namespace clinica_dental_api_cloud.DTOs.Patients;

/// <summary>
/// Ficha del paciente para "Mi perfil". BirthDate es una fecha sola (AAAA-MM-DD); DataConsentAt sale en hora de
/// Costa Rica con su desfase, como el resto de fechas del API.
/// </summary>
public record PatientProfileResponse(
    long Id,
    string FullName,
    string Identification,
    DateOnly? BirthDate,
    string? Phone,
    string? Email,
    string? EmergencyContact,
    DateTime? DataConsentAt,
    string Status);
