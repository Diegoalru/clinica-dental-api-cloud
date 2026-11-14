using clinica_dental_api_cloud.Services;

namespace clinica_dental_api_cloud.Tests;

public class AppointmentTransitionPolicyTests
{
    private static readonly DateTime Now = new(2026, 11, 13, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Future = Now.AddHours(2);
    private static readonly DateTime Past = Now.AddHours(-1);

    [Theory]
    [InlineData(Roles.Admin)]
    [InlineData(Roles.Receptionist)]
    public void StaffCanConfirmAndCancelFutureAppointment(string role)
    {
        Assert.True(AppointmentTransitionPolicy.Allows(
            role, AppointmentStatus.Pending, AppointmentStatus.Confirmed, Future, Now));
        Assert.True(AppointmentTransitionPolicy.Allows(
            role, AppointmentStatus.Confirmed, AppointmentStatus.Cancelled, Future, Now));
    }

    [Fact]
    public void DentistCannotConfirmOrCancel()
    {
        Assert.False(AppointmentTransitionPolicy.Allows(
            Roles.Dentist, AppointmentStatus.Pending, AppointmentStatus.Confirmed, Future, Now));
        Assert.False(AppointmentTransitionPolicy.Allows(
            Roles.Dentist, AppointmentStatus.Confirmed, AppointmentStatus.Cancelled, Future, Now));
    }

    [Fact]
    public void PatientCannotChangeStatus()
    {
        Assert.False(AppointmentTransitionPolicy.Allows(
            Roles.Patient, AppointmentStatus.Pending, AppointmentStatus.Cancelled, Future, Now));
    }

    [Fact]
    public void DentistStartsAndCompletesAnAppointment()
    {
        Assert.True(AppointmentTransitionPolicy.Allows(
            Roles.Dentist, AppointmentStatus.Confirmed, AppointmentStatus.InProgress, Past, Now));
        Assert.True(AppointmentTransitionPolicy.Allows(
            Roles.Dentist, AppointmentStatus.InProgress, AppointmentStatus.Completed, Past, Now));
    }

    [Fact]
    public void NoShowOnlyAfterAppointmentStarts()
    {
        Assert.False(AppointmentTransitionPolicy.Allows(
            Roles.Receptionist, AppointmentStatus.Confirmed, AppointmentStatus.NoShow, Future, Now));
        Assert.True(AppointmentTransitionPolicy.Allows(
            Roles.Receptionist, AppointmentStatus.Confirmed, AppointmentStatus.NoShow, Past, Now));
    }

    [Fact]
    public void TerminalStatusCannotBeReopened()
    {
        foreach (var status in new[]
                 { AppointmentStatus.Cancelled, AppointmentStatus.Completed, AppointmentStatus.NoShow })
            Assert.False(AppointmentTransitionPolicy.Allows(
                Roles.Admin, status, AppointmentStatus.Confirmed, Future, Now));
    }

    [Fact]
    public void PastAppointmentCannotBeConfirmedOrCancelled()
    {
        Assert.False(AppointmentTransitionPolicy.Allows(
            Roles.Admin, AppointmentStatus.Pending, AppointmentStatus.Confirmed, Past, Now));
        Assert.False(AppointmentTransitionPolicy.Allows(
            Roles.Admin, AppointmentStatus.Pending, AppointmentStatus.Cancelled, Past, Now));
    }
}
