namespace clinica_dental_api_cloud.DTOs.Appointments;

/// <summary>
/// Un espacio del día. Time es la hora de inicio en hora de Costa Rica ("09:15"); Available indica si se puede
/// agendar. No incluye datos de la cita que lo ocupa.
/// </summary>
public record AvailabilitySlotResponse(string Time, bool Available);
