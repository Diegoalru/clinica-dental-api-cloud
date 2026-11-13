using System.ComponentModel.DataAnnotations;
using clinica_dental_api_cloud.Services;

namespace clinica_dental_api_cloud.DTOs.Auth;

/// <summary>
///     Cuenta del personal creada por un administrador. Un DENTIST también recibe su perfil profesional en dentists.
/// </summary>
public class CreateStaffRequest : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(255, ErrorMessage = "El correo no puede superar los 255 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(ValidationPatterns.Password, ErrorMessage = ValidationPatterns.PasswordMessage)]
    public string Password { get; set; } = string.Empty;

    [RegularExpression(ValidationPatterns.Phone, ErrorMessage = ValidationPatterns.PhoneMessage)]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [AllowedValues(Roles.Admin, Roles.Receptionist, Roles.Dentist,
        ErrorMessage = "El rol debe ser ADMIN, RECEPTIONIST o DENTIST.")]
    public string Role { get; set; } = string.Empty;

    /// <summary>Solo para DENTIST.</summary>
    [StringLength(30, ErrorMessage = "El número de colegiado no puede superar los 30 caracteres.")]
    public string? LicenseNumber { get; set; }

    /// <summary>Solo para DENTIST.</summary>
    public short? IdSpecialty { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Role != Roles.Dentist)
            yield break;

        if (string.IsNullOrWhiteSpace(LicenseNumber))
            yield return new ValidationResult("El número de colegiado es obligatorio para un odontólogo.", [nameof(LicenseNumber)]);

        if (IdSpecialty is null)
            yield return new ValidationResult("La especialidad es obligatoria para un odontólogo.", [nameof(IdSpecialty)]);
    }
}
