using clinica_dental_api_cloud.DTOs;
using clinica_dental_api_cloud.DTOs.Appointments;

namespace clinica_dental_api_cloud.Services;

public interface IAppointmentService
{
    /// <summary>
    /// Creates a PENDING appointment with its creation log and notifications, in one transaction.
    /// </summary>
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Appointments the current user may see: staff sees all, a dentist or patient only their own.
    /// </summary>
    Task<PagedResponse<AppointmentResponse>> ListAsync(AppointmentQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// 404 when it doesn't exist or the current user may not see it.
    /// </summary>
    Task<AppointmentDetailResponse> GetAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Cambia el estado (reglas en AppointmentStatusRules) y lo registra en appointment_logs, en una transacción.
    /// Al cancelar, anula los avisos pendientes y programa el de cancelación.
    /// </summary>
    Task<AppointmentDetailResponse> ChangeStatusAsync(long id, UpdateAppointmentStatusRequest request,
        CancellationToken cancellationToken);

    Task<AppointmentDetailResponse> RescheduleAsync(long id, RescheduleAppointmentRequest request,
        CancellationToken cancellationToken);

    Task<AppointmentDetailResponse> ResendConfirmationAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    /// Espacios de un día para un odontólogo y un servicio, cruzando su horario, bloqueos y citas no canceladas.
    /// </summary>
    Task<IReadOnlyList<AvailabilitySlotResponse>> GetAvailabilityAsync(AvailabilityQuery query,
        CancellationToken cancellationToken);
}
