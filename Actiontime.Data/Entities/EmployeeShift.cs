using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class EmployeeShift
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LocationId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public DateTime ShiftStart { get; set; }

    public DateTime? ShiftEnd { get; set; }

    public string? Duration { get; set; }

    public TimeOnly? ShiftDuration { get; set; }

    public int? DurationMinute { get; set; }

    public int RecordEmployeeId { get; set; }

    public DateTime RecordDate { get; set; }

    public Guid Uid { get; set; }
}
