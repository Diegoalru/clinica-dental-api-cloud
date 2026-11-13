using System.ComponentModel.DataAnnotations;

namespace clinica_dental_api_cloud.DTOs.Auth;

/// <summary>
///     Registro público: crea una cuenta PATIENT y su ficha en patients.
/// </summary>
public class RegisterPatientRequest : IValidatableObject
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [RegularExpression(ValidationPatterns.Identification, ErrorMessage = ValidationPatterns.IdentificationMessage)]
    public string Identification { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(255, ErrorMessage = "El correo no puede superar los 255 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [RegularExpression(ValidationPatterns.Password, ErrorMessage = ValidationPatterns.PasswordMessage)]
    public string Password { get; set; } = string.Empty;

    [RegularExpression(ValidationPatterns.Phone, ErrorMessage = ValidationPatterns.PhoneMessage)]
    public string? Phone { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(200, ErrorMessage = "El contacto de emergencia no puede superar los 200 caracteres.")]
    public string? EmergencyContact { get; set; }

    /// <summary>El paciente debe aceptar el tratamiento de sus datos; la fecha y hora se guardan en data_consent_at.</summary>
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar el tratamiento de sus datos personales.")]
    public bool AcceptsDataConsent { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (BirthDate is { } birthDate && birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            yield return new ValidationResult("La fecha de nacimiento no puede estar en el futuro.", [nameof(BirthDate)]);
    }
}
