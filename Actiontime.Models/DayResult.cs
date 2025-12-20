namespace Actiontime.Models
{
    public class DayResult
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public DateOnly Date { get; set; }
        public int StateId { get; set; }
        public string StateName { get; set; }
        public int? EnvironmentId { get; set; }
        public string? Description { get; set; }
        public string? PhotoFile { get; set; }
        public int? RecordEmployeeId { get; set; }
        public DateTime? RecordDate { get; set; }
        public int? UpdateEmployeeId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Uid { get; set; }
    }
}