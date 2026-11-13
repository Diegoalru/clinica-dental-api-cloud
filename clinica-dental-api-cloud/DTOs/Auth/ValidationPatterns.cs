namespace clinica_dental_api_cloud.DTOs.Auth;

/// <summary>
///     Reglas compartidas por los formularios de cuentas, para que el registro de pacientes y el del personal validen igual.
/// </summary>
public static class ValidationPatterns
{
    // Entre 8 y 72 caracteres, con al menos una letra y un número. El máximo es 72 porque bcrypt ignora lo que
    // pase de 72 bytes. El largo va en la misma expresión para que el error salga una sola vez.
    public const string Password = @"^(?=.*[A-Za-zÁÉÍÓÚÑáéíóúñ])(?=.*\d).{8,72}$";
    public const string PasswordMessage = "La contraseña debe tener entre 8 y 72 caracteres, con al menos una letra y un número.";

    // Permisiva a propósito: acepta cédula nacional (9 dígitos), DIMEX y DIDI (11-12 dígitos) y pasaportes
    // (alfanuméricos). La cédula nacional se normaliza después en IdentificationFormat.
    public const string Identification = @"^[A-Za-z0-9-]{5,30}$";
    public const string IdentificationMessage =
        "La identificación debe tener entre 5 y 30 caracteres: letras, números y guiones (cédula, DIMEX o pasaporte).";

    public const string Phone = @"^[0-9+\-\s()]{7,30}$";
    public const string PhoneMessage = "El teléfono solo puede tener números, espacios, +, - y paréntesis (7 a 30 caracteres).";
}
