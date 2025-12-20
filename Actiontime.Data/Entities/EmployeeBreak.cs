using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class EmployeeBreak
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int LocationId { get; set; }

    public DateOnly BreakDate { get; set; }

    public DateTime BreakStart { get; set; }

    public DateTime? BreakEnd { get; set; }

    public string? Duration { get; set; }

    public TimeOnly? BreakDuration { get; set; }

    public int? DurationMinute { get; set; }

    public int RecordEmployeeId { get; set; }

    public DateTime RecordDate { get; set; }

    public Guid Uid { get; set; }
}
