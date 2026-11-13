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
    Task<AppointmentResponse> GetAsync(long id, CancellationToken cancellationToken);
}
