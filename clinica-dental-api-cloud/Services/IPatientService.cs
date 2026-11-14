using clinica_dental_api_cloud.DTOs.Patients;

namespace clinica_dental_api_cloud.Services;

public interface IPatientService
{
    /// <summary>
    /// Active patients whose name or identification contains the text (up to 20, by name).
    /// </summary>
    Task<IReadOnlyList<PatientResponse>> SearchAsync(string? search, CancellationToken cancellationToken);

    /// <summary>
    /// Ficha del paciente vinculado al usuario de la sesión. 404 si la cuenta no tiene paciente.
    /// </summary>
    Task<PatientProfileResponse> GetOwnProfileAsync(CancellationToken cancellationToken);
}
