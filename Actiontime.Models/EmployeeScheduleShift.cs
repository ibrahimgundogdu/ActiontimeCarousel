namespace Actiontime.Models
{
    public class EmployeeScheduleShift
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ScheduleTime { get; set; }
        public string? ScheduleDuration { get; set; }
        public string? ShiftTime { get; set; }
        public string? ShiftDuration { get; set; }
        public bool IsValid { get; set; } = false;
    }
}