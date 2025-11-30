using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class ScheduleEmployee
{
    public long Id { get; set; }

    public int StatusId { get; set; }

    public int? EmployeeId { get; set; }

    public int? LocationId { get; set; }

    public short TimeZone { get; set; }

    public DateTime? ShiftStart { get; set; }

    public DateTime? ShiftEnd { get; set; }

    public int? DurationMinute { get; set; }

    public TimeSpan? ShiftDuration { get; set; }

    public double? UnitPriceMultiplier { get; set; }

    public Guid? Uid { get; set; }
}
