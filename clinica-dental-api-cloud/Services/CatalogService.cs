using System.Globalization;
using clinica_dental_api_cloud.Data;
using clinica_dental_api_cloud.DTOs.Catalog;
using clinica_dental_api_cloud.Errors;
using Microsoft.EntityFrameworkCore;

namespace clinica_dental_api_cloud.Services;

public class CatalogService(ClinicaDbContext db, ICurrentUser currentUser) : ICatalogService
{
    // Bloqueos que se devuelven: los que terminaron hace menos de una semana y los futuros (la agenda es semanal)
    private static readonly TimeSpan PastBlocksWindow = TimeSpan.FromDays(7);

    public async Task<DentistProfileResponse> GetOwnDentistAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var since = DateTime.UtcNow - PastBlocksWindow;

        var dentist = await db.Dentists
                          .AsNoTracking()
                          .Where(d => d.IdUser == userId)
                          .Select(d => new
                          {
                              d.IdDentist,
                              d.IdUserNavigation.FullName,
                              Specialty = d.IdSpecialtyNavigation.Name,
                              d.LicenseNumber,
                              Schedules = d.DentistSchedules
                                  .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                                  .Select(s => new { s.DayOfWeek, s.StartTime, s.EndTime })
                                  .ToList(),
                              Blocks = d.DentistBlocks
                                  .Where(b => b.EndsAt >= since)
                                  .OrderBy(b => b.StartsAt)
                                  .Select(b => new DentistBlockResponse(b.IdBlock, b.StartsAt, b.EndsAt, b.Reason))
                                  .ToList()
                          })
                          .AsSplitQuery()
                          .FirstOrDefaultAsync(cancellationToken)
                      ?? throw ApiException.NotFound("Su usuario no está vinculado a un odontólogo.");

        // TimeOnly saldría "08:00:00"; el frontend trabaja con "08:00"
        var schedules = dentist.Schedules
            .Select(s => new DentistScheduleResponse(s.DayOfWeek,
                s.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
                s.EndTime.ToString("HH:mm", CultureInfo.InvariantCulture)))
            .ToList();

        return new DentistProfileResponse(dentist.IdDentist, dentist.FullName, dentist.Specialty,
            dentist.LicenseNumber, schedules, dentist.Blocks);
    }

    public async Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken)
    {
        return await db.Services
            .AsNoTracking()
            .Where(s => s.Status == RecordStatus.Active)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceResponse(s.IdService, s.Name, s.Description, s.DurationMinutes, s.Price))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DentistResponse>> GetDentistsAsync(long? serviceId,
        CancellationToken cancellationToken)
    {
        var query = db.Dentists
            .AsNoTracking()
            .Where(d => d.Status == RecordStatus.Active && d.IdUserNavigation.Status == RecordStatus.Active);

        if (serviceId is not null)
            query = query.Where(d => d.DentistServices.Any(ds => ds.IdService == serviceId));

        return await query
            .OrderBy(d => d.IdUserNavigation.FullName)
            .Select(d => new DentistResponse(
                d.IdDentist,
                d.IdUserNavigation.FullName,
                d.IdSpecialtyNavigation.Name,
                d.LicenseNumber))
            .ToListAsync(cancellationToken);
    }
}
