using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class EmployeeSchedule
{
    public long Id { get; set; }

    public int EmployeeId { get; set; }

    public int LocationId { get; set; }

    public short TimeZone { get; set; }

    public string ScheduleWeek { get; set; } = null!;

    public DateTime ShiftStart { get; set; }

    public DateTime? ShiftEnd { get; set; }

    public DateOnly? ScheduleDate { get; set; }

    public int? DurationMinute { get; set; }

    public TimeOnly? ShiftDuration { get; set; }

    public double? UnitPriceMultiplier { get; set; }

    public Guid Uid { get; set; }
}
