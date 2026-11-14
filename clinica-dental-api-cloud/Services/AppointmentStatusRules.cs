using clinica_dental_api_cloud.Errors;
using static clinica_dental_api_cloud.Services.AppointmentStatus;

namespace clinica_dental_api_cloud.Services;

/// <summary>
/// Qué cambios de estado se permiten, quién los puede hacer y cuándo. No toca la base, para probarlo con xUnit.
/// </summary>
public static class AppointmentStatusRules
{
    private static readonly Dictionary<string, string> Labels = new()
    {
        [Pending] = "pendiente",
        [Confirmed] = "confirmada",
        [InProgress] = "en atención",
        [Completed] = "completada",
        [Cancelled] = "cancelada",
        [NoShow] = "no asistió"
    };

    // Desde cada estado, a cuáles puede pasar. COMPLETED, CANCELLED y NO_SHOW son finales; IN_PROGRESS pasa a
    // COMPLETED solo al guardar el expediente.
    private static readonly Dictionary<string, string[]> Transitions = new()
    {
        [Pending] = [Confirmed, Cancelled, NoShow],
        [Confirmed] = [InProgress, Cancelled, NoShow],
        [InProgress] = [],
        [Completed] = [],
        [Cancelled] = [],
        [NoShow] = []
    };

    // Qué estados puede poner cada rol (el odontólogo y el paciente, solo en sus propias citas)
    private static readonly Dictionary<string, string[]> AllowedByRole = new()
    {
        [Roles.Admin] = [Confirmed, InProgress, Cancelled, NoShow],
        [Roles.Receptionist] = [Confirmed, Cancelled, NoShow],
        [Roles.Dentist] = [InProgress, NoShow],
        [Roles.Patient] = [Cancelled]
    };

    public static string Label(string status) => Labels.GetValueOrDefault(status, status);

    /// <summary>
    /// Lanza ApiException si el cambio no se permite: 400 (dato inválido), 403 (rol) o 409 (estado u hora).
    /// </summary>
    public static void EnsureCanChange(string role, string current, string target, DateTime startsAtUtc,
        DateTime nowUtc)
    {
        if (!All.Contains(target))
            throw ApiException.BadRequest("El estado no es válido.");

        if (target == Completed)
            throw ApiException.BadRequest("La cita se completa al guardar el expediente clínico.");

        if (!AllowedByRole.TryGetValue(role, out var allowed) || !allowed.Contains(target))
            throw ApiException.Forbidden($"No tiene permiso para marcar la cita como {Label(target)}.");

        if (!Transitions.TryGetValue(current, out var next) || !next.Contains(target))
            throw ApiException.Conflict($"La cita está {Label(current)} y no puede pasar a {Label(target)}.");

        var started = startsAtUtc <= nowUtc;

        if (target is Confirmed or Cancelled && started)
            throw ApiException.Conflict("La cita ya empezó: no se puede confirmar ni cancelar.");

        if (target == NoShow && !started)
            throw ApiException.Conflict("Solo se marca que no asistió cuando ya pasó la hora de la cita.");
    }
}
