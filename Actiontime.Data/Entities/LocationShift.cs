using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class LocationShift
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public int? EmployeeIdfinish { get; set; }

    public DateTime ShiftDate { get; set; }

    public DateTime ShiftStart { get; set; }

    public DateTime? ShiftFinish { get; set; }

    public string? Duration { get; set; }

    public int? DurationMinute { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public TimeSpan? ShiftDuration { get; set; }

    public Guid Uid { get; set; }
}
