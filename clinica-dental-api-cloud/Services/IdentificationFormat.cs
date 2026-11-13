namespace clinica_dental_api_cloud.Services;

/// <summary>
///     Deja la identificación en un solo formato antes de guardarla o buscarla, para que la misma persona no quede
///     registrada dos veces por escribirla distinto.
/// </summary>
public static class IdentificationFormat
{
    /// <summary>
    ///     Una cédula nacional (9 dígitos, con o sin guiones) queda como X-XXXX-XXXX, igual que en el seed.
    ///     DIMEX, DIDI y pasaportes se guardan como vienen, sin espacios y con las letras en mayúscula.
    /// </summary>
    public static string Normalize(string identification)
    {
        var value = identification.Trim().ToUpperInvariant();
        var digits = value.Replace("-", "");

        return digits.Length == 9 && digits.All(char.IsAsciiDigit)
            ? $"{digits[0]}-{digits[1..5]}-{digits[5..]}"
            : value;
    }
}
