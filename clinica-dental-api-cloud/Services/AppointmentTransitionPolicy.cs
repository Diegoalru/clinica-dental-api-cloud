namespace clinica_dental_api_cloud.Services;

public static class AppointmentTransitionPolicy
{
    public static bool Allows(string role, string current, string target, DateTime startsAt, DateTime now)
    {
        var staff = role is Roles.Admin or Roles.Receptionist;
        var dentist = role == Roles.Dentist;

        return target switch
        {
            AppointmentStatus.Confirmed => staff && current == AppointmentStatus.Pending && startsAt > now,
            AppointmentStatus.Cancelled => staff &&
                (current is AppointmentStatus.Pending or AppointmentStatus.Confirmed) && startsAt > now,
            AppointmentStatus.InProgress => dentist &&
                current == AppointmentStatus.Confirmed && startsAt <= now,
            AppointmentStatus.Completed => dentist && current == AppointmentStatus.InProgress && startsAt <= now,
            AppointmentStatus.NoShow => (staff || dentist) &&
                (current is AppointmentStatus.Pending or AppointmentStatus.Confirmed) && startsAt <= now,
            _ => false
        };
    }
}
