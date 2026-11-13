namespace clinica_dental_api_cloud.Services;

/// <summary>
///     Nombres de los roles tal como están en roles.name. Son los que van en el claim "role" del token y en [Authorize(Roles = ...)].
/// </summary>
public static class AppRoles
{
    public const string Admin = "ADMIN";
    public const string Receptionist = "RECEPTIONIST";
    public const string Dentist = "DENTIST";
    public const string Patient = "PATIENT";
}

/// <summary>
///     Valores de la columna status en users, patients, dentists y services.
/// </summary>
public static class RecordStatus
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
}
