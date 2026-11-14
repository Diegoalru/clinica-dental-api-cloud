using System.Globalization;
using System.Linq.Expressions;
using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.Data.Entities;
using clinica_dental_api_cloud.DTOs;
using clinica_dental_api_cloud.DTOs.Appointments;
using clinica_dental_api_cloud.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace clinica_dental_api_cloud.Services;

public class AppointmentService(ClinicaDbContext db, ICurrentUser currentUser) : IAppointmentService
{
    private static readonly TimeSpan ReminderLeadTime = TimeSpan.FromHours(24);

    private static readonly Expression<Func<Appointment, AppointmentResponse>> ToResponse = a =>
        new AppointmentResponse(
            a.IdAppointment,
            a.IdPatient,
            a.IdPatientNavigation.FullName,
            a.IdDentist,
            a.DentistService.IdDentistNavigation.IdUserNavigation.FullName,
            a.IdService,
            a.DentistService.IdServiceNavigation.Name,
            a.StartsAt,
            a.EndsAt,
            a.Status,
            a.PriceSnapshot,
            a.CreatedAt);

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var dentistId = request.DentistId!.Value;
        var serviceId = request.ServiceId!.Value;
        var startsAt = ClinicTime.ToUtc(request.StartsAt!.Value);
        var now = DateTime.UtcNow;

        if (startsAt <= now)
            throw ApiException.BadRequest("La cita debe ser en una fecha y hora futura.");

        // Mismas horas que ofrece GET /appointments/availability
        if (!AvailabilityCalculator.IsSlotStart(startsAt))
            throw ApiException.BadRequest(
                "La cita debe empezar en un intervalo de 15 minutos (por ejemplo 09:00, 09:15 o 09:30).");

        var patientId = await ResolvePatientIdAsync(request.PatientId, cancellationToken);

        var patient = await db.Patients
                          .AsNoTracking()
                          .Where(p => p.IdPatient == patientId)
                          .Select(p => new { p.Status, Email = p.Email ?? p.IdUserNavigation!.Email })
                          .FirstOrDefaultAsync(cancellationToken)
                      ?? throw ApiException.NotFound("El paciente no existe.");

        if (patient.Status != RecordStatus.Active)
            throw ApiException.BadRequest("El paciente está inactivo.");

        // The pair must exist in dentist_services (the appointment's composite FK)
        var offering = await db.DentistServices
                           .AsNoTracking()
                           .Where(ds => ds.IdDentist == dentistId && ds.IdService == serviceId)
                           .Select(ds => new
                           {
                               DentistStatus = ds.IdDentistNavigation.Status,
                               UserStatus = ds.IdDentistNavigation.IdUserNavigation.Status,
                               ServiceStatus = ds.IdServiceNavigation.Status,
                               ds.IdServiceNavigation.DurationMinutes,
                               ds.IdServiceNavigation.Price
                           })
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw ApiException.BadRequest("El odontólogo no ofrece ese servicio.");

        // Igual que GET /dentists y la disponibilidad: la ficha y la cuenta del odontólogo deben estar activas
        if (offering.DentistStatus != RecordStatus.Active || offering.UserStatus != RecordStatus.Active)
            throw ApiException.BadRequest("El odontólogo está inactivo.");
        if (offering.ServiceStatus != RecordStatus.Active)
            throw ApiException.BadRequest("El servicio está inactivo.");

        // The backend calculates the end, never the client
        var endsAt = startsAt.AddMinutes(offering.DurationMinutes);

        await EnsureWithinScheduleAsync(dentistId, startsAt, endsAt, cancellationToken);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var blocked = await db.DentistBlocks.AnyAsync(
            b => b.IdDentist == dentistId && b.StartsAt < endsAt && startsAt < b.EndsAt, cancellationToken);
        if (blocked)
            throw ApiException.Conflict("El odontólogo no está disponible en ese horario.");

        var appointment = new Appointment
        {
            IdPatient = patientId,
            IdDentist = dentistId,
            IdService = serviceId,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = AppointmentStatus.Pending,
            PriceSnapshot = offering.Price
        };

        appointment.AppointmentLogs.Add(new AppointmentLog
        {
            ChangedBy = currentUser.UserId,
            PreviousStatus = null,
            NewStatus = AppointmentStatus.Pending,
            Remarks = "Cita creada"
        });

        AddNotifications(appointment, patient.Email, now);

        db.Appointments.Add(appointment);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ExclusionViolation
        })
        {
            // appointments_no_overlap: another active appointment of this dentist overlaps
            throw ApiException.Conflict("El espacio ya fue tomado. Elija otro horario.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation
        })
        {
            throw ApiException.BadRequest("El paciente, el odontólogo o el servicio no existen.");
        }

        return await db.Appointments
            .AsNoTracking()
            .Where(a => a.IdAppointment == appointment.IdAppointment)
            .Select(ToResponse)
            .FirstAsync(cancellationToken);
    }

    public async Task<PagedResponse<AppointmentResponse>> ListAsync(AppointmentQuery query,
        CancellationToken cancellationToken)
    {
        var appointments = VisibleAppointments();

        if (query.From is not null)
        {
            var from = ClinicTime.ToUtc(query.From.Value);
            appointments = appointments.Where(a => a.StartsAt >= from);
        }

        if (query.To is not null)
        {
            var to = ClinicTime.ToUtc(query.To.Value);
            appointments = appointments.Where(a => a.StartsAt < to);
        }

        if (query.DentistId is not null)
            appointments = appointments.Where(a => a.IdDentist == query.DentistId);

        if (query.PatientId is not null)
            appointments = appointments.Where(a => a.IdPatient == query.PatientId);

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToUpperInvariant();
            if (!AppointmentStatus.All.Contains(status))
                throw ApiException.BadRequest("El estado no es válido.");

            appointments = appointments.Where(a => a.Status == status);
        }

        var totalCount = await appointments.CountAsync(cancellationToken);

        var items = await appointments
            .OrderByDescending(a => a.StartsAt)
            .ThenByDescending(a => a.IdAppointment)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ToResponse)
            .ToListAsync(cancellationToken);

        return new PagedResponse<AppointmentResponse>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<AppointmentDetailResponse> GetAsync(long id, CancellationToken cancellationToken)
    {
        return await VisibleAppointments()
                   .Where(a => a.IdAppointment == id)
                   .Select(a => new AppointmentDetailResponse(
                       a.IdAppointment,
                       a.IdPatient,
                       a.IdPatientNavigation.FullName,
                       a.IdPatientNavigation.Identification,
                       a.IdPatientNavigation.Phone,
                       a.IdPatientNavigation.Email ?? a.IdPatientNavigation.IdUserNavigation!.Email,
                       a.IdDentist,
                       a.DentistService.IdDentistNavigation.IdUserNavigation.FullName,
                       a.IdService,
                       a.DentistService.IdServiceNavigation.Name,
                       a.StartsAt,
                       a.EndsAt,
                       a.Status,
                       a.PriceSnapshot,
                       a.CreatedAt,
                       a.AppointmentLogs
                           .OrderBy(l => l.ChangedAt).ThenBy(l => l.IdLog)
                           .Select(l => new AppointmentLogResponse(
                               l.IdLog, l.PreviousStatus, l.NewStatus, l.ChangedAt,
                               l.ChangedByNavigation.FullName, l.Remarks))
                           .ToList(),
                       a.Notifications
                           .OrderBy(n => n.ScheduledFor).ThenBy(n => n.IdNotification)
                           .Select(n => new NotificationResponse(
                               n.IdNotification, n.Type, n.Channel, n.Status, n.ScheduledFor, n.SentAt))
                           .ToList()))
                   .AsSplitQuery()
                   .FirstOrDefaultAsync(cancellationToken)
               ?? throw ApiException.NotFound("La cita no existe.");
    }

    public async Task<AppointmentDetailResponse> ChangeStatusAsync(long id, UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var target = request.Status!.Trim().ToUpperInvariant();
        var remarks = string.IsNullOrWhiteSpace(request.Remarks) ? null : request.Remarks.Trim();

        if (target == AppointmentStatus.Cancelled && remarks is null)
            throw ApiException.BadRequest("Indique el motivo de la cancelación.");

        // 404 también si la cita existe pero el usuario no la puede ver (otro paciente u otro odontólogo)
        if (!await VisibleAppointments().AnyAsync(a => a.IdAppointment == id, cancellationToken))
            throw ApiException.NotFound("La cita no existe.");

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // FOR UPDATE: si dos personas cambian la misma cita a la vez, la segunda espera y ve el estado nuevo
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM appointments WHERE id_appointment = {id} FOR UPDATE", cancellationToken);

        var appointment = await db.Appointments
            .Include(a => a.Notifications)
            .FirstAsync(a => a.IdAppointment == id, cancellationToken);

        var now = DateTime.UtcNow;
        AppointmentStatusRules.EnsureCanChange(currentUser.Role, appointment.Status, target, appointment.StartsAt, now);

        appointment.AppointmentLogs.Add(new AppointmentLog
        {
            ChangedBy = currentUser.UserId,
            PreviousStatus = appointment.Status,
            NewStatus = target,
            Remarks = remarks
        });
        appointment.Status = target;
        appointment.UpdatedAt = now;

        if (target == AppointmentStatus.Cancelled)
            CancelNotifications(appointment, now);

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    /// <summary>
    /// Al cancelar, los avisos pendientes (el recordatorio) ya no se envían y se programa el aviso de cancelación
    /// al mismo correo de la confirmación.
    /// </summary>
    private static void CancelNotifications(Appointment appointment, DateTime now)
    {
        foreach (var notification in appointment.Notifications.Where(n => n.Status == "PENDING"))
            notification.Status = "CANCELLED";

        var recipient = appointment.Notifications.Select(n => n.Recipient).FirstOrDefault();
        if (recipient is null)
            return;

        appointment.Notifications.Add(new Notification
        {
            Type = "CANCELLATION",
            Channel = "EMAIL",
            Recipient = recipient,
            Status = "PENDING",
            ScheduledFor = now
        });
    }

    public async Task<IReadOnlyList<AvailabilitySlotResponse>> GetAvailabilityAsync(AvailabilityQuery query,
        CancellationToken cancellationToken)
    {
        var dentistId = query.DentistId!.Value;
        var serviceId = query.ServiceId!.Value;
        var date = query.Date!.Value;

        var offering = await db.DentistServices
                           .AsNoTracking()
                           .Where(ds => ds.IdDentist == dentistId && ds.IdService == serviceId)
                           .Select(ds => new
                           {
                               DentistStatus = ds.IdDentistNavigation.Status,
                               UserStatus = ds.IdDentistNavigation.IdUserNavigation.Status,
                               ServiceStatus = ds.IdServiceNavigation.Status,
                               ds.IdServiceNavigation.DurationMinutes
                           })
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw ApiException.BadRequest("El odontólogo no ofrece ese servicio.");

        // Igual que GET /dentists: la ficha y la cuenta del odontólogo deben estar activas
        if (offering.DentistStatus != RecordStatus.Active || offering.UserStatus != RecordStatus.Active)
            throw ApiException.BadRequest("El odontólogo está inactivo.");
        if (offering.ServiceStatus != RecordStatus.Active)
            throw ApiException.BadRequest("El servicio está inactivo.");

        // El día de la semana es el de la fecha en Costa Rica, no el de UTC
        var day = ClinicTime.IsoDayOfWeek(date.ToDateTime(TimeOnly.MinValue));
        var periods = await db.DentistSchedules
            .AsNoTracking()
            .Where(s => s.IdDentist == dentistId && s.DayOfWeek == day)
            .Select(s => new { s.StartTime, s.EndTime })
            .ToListAsync(cancellationToken);

        if (periods.Count == 0)
            return [];

        // El día completo en Costa Rica, pasado a UTC para comparar con la base
        var dayStart = ClinicTime.ToUtc(date.ToDateTime(TimeOnly.MinValue));
        var dayEnd = ClinicTime.ToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue));

        // Ocupan el espacio todas las citas menos las canceladas, como en appointments_no_overlap
        var appointments = await db.Appointments
            .AsNoTracking()
            .Where(a => a.IdDentist == dentistId && a.Status != AppointmentStatus.Cancelled
                        && a.StartsAt < dayEnd && dayStart < a.EndsAt)
            .Select(a => new { a.StartsAt, a.EndsAt })
            .ToListAsync(cancellationToken);

        var blocks = await db.DentistBlocks
            .AsNoTracking()
            .Where(b => b.IdDentist == dentistId && b.StartsAt < dayEnd && dayStart < b.EndsAt)
            .Select(b => new { b.StartsAt, b.EndsAt })
            .ToListAsync(cancellationToken);

        var busy = appointments.Select(a => (a.StartsAt, a.EndsAt))
            .Concat(blocks.Select(b => (b.StartsAt, b.EndsAt)))
            .ToList();

        var slots = AvailabilityCalculator.Compute(
            date,
            offering.DurationMinutes,
            periods.Select(p => (p.StartTime, p.EndTime)),
            busy,
            DateTime.UtcNow);

        return slots
            .Select(slot => new AvailabilitySlotResponse(slot.Time.ToString("HH:mm", CultureInfo.InvariantCulture),
                slot.Available))
            .ToList();
    }

    /// <summary>
    /// Staff sees every appointment; a dentist or a patient only their own.
    /// </summary>
    private IQueryable<Appointment> VisibleAppointments()
    {
        var appointments = db.Appointments.AsNoTracking();
        var userId = currentUser.UserId;

        return currentUser.Role switch
        {
            Roles.Admin or Roles.Receptionist => appointments,
            Roles.Dentist => appointments.Where(a => a.DentistService.IdDentistNavigation.IdUser == userId),
            Roles.Patient => appointments.Where(a => a.IdPatientNavigation.IdUser == userId),
            _ => throw ApiException.Forbidden("No tiene permiso para ver citas.")
        };
    }

    /// <summary>
    /// A patient books only for their own record; staff must say which patient.
    /// </summary>
    private async Task<long> ResolvePatientIdAsync(long? requestedPatientId, CancellationToken cancellationToken)
    {
        if (currentUser.Role != Roles.Patient)
            return requestedPatientId ?? throw ApiException.BadRequest("El paciente es obligatorio.");

        var userId = currentUser.UserId;
        var ownPatientId = await db.Patients
                               .Where(p => p.IdUser == userId)
                               .Select(p => (long?)p.IdPatient)
                               .FirstOrDefaultAsync(cancellationToken)
                           ?? throw ApiException.Forbidden("Su usuario no está vinculado a un paciente.");

        if (requestedPatientId is not null && requestedPatientId != ownPatientId)
            throw ApiException.Forbidden("Solo puede agendar citas para usted.");

        return ownPatientId;
    }

    /// <summary>
    /// The appointment must fit completely inside one of the dentist's working periods for that day
    /// (dentist_schedules uses clinic local time).
    /// </summary>
    private async Task EnsureWithinScheduleAsync(long dentistId, DateTime startsAt, DateTime endsAt,
        CancellationToken cancellationToken)
    {
        var localStart = ClinicTime.ToClinic(startsAt);
        var localEnd = ClinicTime.ToClinic(endsAt);

        if (localEnd.Date != localStart.Date)
            throw ApiException.BadRequest("La cita debe empezar y terminar el mismo día.");

        var day = ClinicTime.IsoDayOfWeek(localStart);
        var start = TimeOnly.FromDateTime(localStart);
        var end = TimeOnly.FromDateTime(localEnd);

        var fits = await db.DentistSchedules.AnyAsync(
            s => s.IdDentist == dentistId && s.DayOfWeek == day && s.StartTime <= start && s.EndTime >= end,
            cancellationToken);

        if (!fits)
            throw ApiException.BadRequest("El horario está fuera de la jornada del odontólogo.");
    }

    /// <summary>
    /// Confirmation now and a reminder 24 h before (skipped if that moment already passed).
    /// The reminder Function sends them; without an email there is nobody to notify.
    /// </summary>
    private static void AddNotifications(Appointment appointment, string? recipient, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            return;

        appointment.Notifications.Add(new Notification
        {
            Type = "CONFIRMATION",
            Channel = "EMAIL",
            Recipient = recipient,
            Status = "PENDING",
            ScheduledFor = now
        });

        var reminderAt = appointment.StartsAt - ReminderLeadTime;
        if (reminderAt > now)
        {
            appointment.Notifications.Add(new Notification
            {
                Type = "REMINDER",
                Channel = "EMAIL",
                Recipient = recipient,
                Status = "PENDING",
                ScheduledFor = reminderAt
            });
        }
    }
}
