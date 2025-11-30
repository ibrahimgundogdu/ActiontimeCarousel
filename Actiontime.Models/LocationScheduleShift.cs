namespace Actiontime.Models
{
    public class LocationScheduleShift
    {
        public long Id { get; set; }
        public string? ScheduleTime { get; set; }
        public string? ScheduleDuration { get; set; }
        public string? ShiftTime { get; set; }
        public string? ShiftDuration { get; set; }
        public bool IsValid { get; set; } = false;
    }
}