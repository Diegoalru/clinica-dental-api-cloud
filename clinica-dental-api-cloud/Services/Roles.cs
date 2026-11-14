namespace clinica_dental_api_cloud.Services;

/// <summary>
/// Role names as stored in the roles table.
/// </summary>
public static class Roles
{
    public const string Admin = "ADMIN";
    public const string Receptionist = "RECEPTIONIST";
    public const string Dentist = "DENTIST";
    public const string Patient = "PATIENT";

    // For [Authorize(Roles = ...)]
    public const string Staff = Admin + "," + Receptionist;
    public const string CanBook = Admin + "," + Receptionist + "," + Patient;
    public const string StaffOrDentist = Staff + "," + Dentist;
}
