namespace clinica_dental_api_cloud.DTOs.Patients;

public record PatientResponse(long Id, string FullName, string Identification, string? Phone, string? Email,
    long? UserId);
