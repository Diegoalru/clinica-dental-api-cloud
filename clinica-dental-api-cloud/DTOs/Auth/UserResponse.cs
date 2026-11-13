namespace clinica_dental_api_cloud.DTOs.Auth;

/// <summary>
///     Datos de la cuenta para el frontend. PatientId y DentistId solo vienen en esos roles, para que la app cargue
///     "mis citas" o "mi agenda" sin otra solicitud.
/// </summary>
public record UserResponse(long Id, string FullName, string Email, string Role, long? PatientId, long? DentistId);
