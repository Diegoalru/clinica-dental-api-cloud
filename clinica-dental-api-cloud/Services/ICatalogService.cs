using clinica_dental_api_cloud.DTOs.Catalog;

namespace clinica_dental_api_cloud.Services;

public interface ICatalogService
{
    /// <summary>
    /// Active services, by name.
    /// </summary>
    Task<IReadOnlyList<ServiceResponse>> GetServicesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Active dentists; with serviceId, only those enabled for that service.
    /// </summary>
    Task<IReadOnlyList<DentistResponse>> GetDentistsAsync(long? serviceId, CancellationToken cancellationToken);

    /// <summary>
    /// El odontólogo vinculado al usuario de la sesión, con su horario y bloqueos. 404 si la cuenta no tiene ficha.
    /// </summary>
    Task<DentistProfileResponse> GetOwnDentistAsync(CancellationToken cancellationToken);
}
