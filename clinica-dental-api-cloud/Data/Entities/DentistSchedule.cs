namespace clinica_dental_api_cloud.Data.Entities;

public class DentistSchedule
{
    public long IdSchedule { get; set; }

    public long IdDentist { get; set; }

    public short DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public virtual Dentist IdDentistNavigation { get; set; } = null!;
}