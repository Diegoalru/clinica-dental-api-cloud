using clinica_dental_api_cloud.Errors;
using clinica_dental_api_cloud.Services;
using static clinica_dental_api_cloud.Services.AppointmentStatus;

namespace clinica_dental_api_cloud.Tests;

/// <summary>
/// Quién puede cambiar el estado de una cita, a cuál y cuándo. 400 dato inválido, 403 rol, 409 estado u hora.
/// </summary>
public class AppointmentStatusRulesTests
{
    private static readonly DateTime Now = new(2026, 11, 20, 15, 0, 0, DateTimeKind.Utc);   // 09:00 Costa Rica
    private static readonly DateTime Tomorrow = Now.AddDays(1);
    private static readonly DateTime OneHourAgo = Now.AddHours(-1);

    private static int StatusCodeOf(Action action) => Assert.Throws<ApiException>(action).StatusCode;

    [Theory]
    [InlineData(Roles.Receptionist, Pending, Confirmed)]
    [InlineData(Roles.Receptionist, Pending, Cancelled)]
    [InlineData(Roles.Receptionist, Confirmed, Cancelled)]
    [InlineData(Roles.Admin, Pending, Confirmed)]
    [InlineData(Roles.Patient, Pending, Cancelled)]
    [InlineData(Roles.Patient, Confirmed, Cancelled)]
    public void AntesDeLaCita_SePuedeConfirmarOCancelar(string role, string current, string target)
    {
        AppointmentStatusRules.EnsureCanChange(role, current, target, Tomorrow, Now);
    }

    [Theory]
    [InlineData(Roles.Receptionist, Pending)]
    [InlineData(Roles.Receptionist, Confirmed)]
    [InlineData(Roles.Dentist, Confirmed)]
    public void DespuesDeLaHora_SeMarcaQueNoAsistio(string role, string current)
    {
        AppointmentStatusRules.EnsureCanChange(role, current, NoShow, OneHourAgo, Now);
    }

    [Fact]
    public void ElOdontologo_PoneEnAtencionUnaCitaConfirmada()
    {
        AppointmentStatusRules.EnsureCanChange(Roles.Dentist, Confirmed, InProgress, OneHourAgo, Now);
    }

    [Theory]
    [InlineData(Roles.Patient, Confirmed)]      // el paciente solo cancela
    [InlineData(Roles.Patient, NoShow)]
    [InlineData(Roles.Dentist, Confirmed)]      // el odontólogo no confirma
    [InlineData(Roles.Dentist, Cancelled)]
    [InlineData(Roles.Receptionist, InProgress)]
    public void RolSinPermiso_Da403(string role, string target)
    {
        var current = target == Confirmed ? Pending : Confirmed;
        Assert.Equal(403, StatusCodeOf(() =>
            AppointmentStatusRules.EnsureCanChange(role, current, target, Tomorrow, Now)));
    }

    [Theory]
    [InlineData(Confirmed, Confirmed)]          // ya estaba confirmada
    [InlineData(Cancelled, Confirmed)]          // una cancelada no se reabre
    [InlineData(NoShow, Cancelled)]
    [InlineData(Completed, Cancelled)]
    [InlineData(InProgress, Cancelled)]
    public void TransicionNoPermitida_Da409(string current, string target)
    {
        Assert.Equal(409, StatusCodeOf(() =>
            AppointmentStatusRules.EnsureCanChange(Roles.Admin, current, target, Tomorrow, Now)));
    }

    [Theory]
    [InlineData(Confirmed)]
    [InlineData(Cancelled)]
    public void CitaQueYaEmpezo_NoSeConfirmaNiCancela(string target)
    {
        // Empieza justo ahora: ya cuenta como empezada
        Assert.Equal(409, StatusCodeOf(() =>
            AppointmentStatusRules.EnsureCanChange(Roles.Receptionist, Pending, target, Now, Now)));
    }

    [Fact]
    public void NoAsistio_AntesDeLaHora_Da409()
    {
        Assert.Equal(409, StatusCodeOf(() =>
            AppointmentStatusRules.EnsureCanChange(Roles.Receptionist, Confirmed, NoShow, Tomorrow, Now)));
    }

    [Theory]
    [InlineData(Completed)]
    [InlineData("FINISHED")]
    [InlineData("")]
    public void EstadoInvalidoOCompletada_Da400(string target)
    {
        Assert.Equal(400, StatusCodeOf(() =>
            AppointmentStatusRules.EnsureCanChange(Roles.Admin, InProgress, target, OneHourAgo, Now)));
    }

    [Fact]
    public void ElMensajeDice_LosEstadosEnEspanol()
    {
        var error = Assert.Throws<ApiException>(() =>
            AppointmentStatusRules.EnsureCanChange(Roles.Admin, Cancelled, Confirmed, Tomorrow, Now));

        Assert.Equal("La cita está cancelada y no puede pasar a confirmada.", error.Message);
    }
}
