using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class ScheduleLocation
{
    public long Id { get; set; }

    public int StatusId { get; set; }

    public int LocationId { get; set; }

    public short TimeZone { get; set; }

    public DateTime ScheduleStart { get; set; }

    public DateTime ScheduleEnd { get; set; }

    public int? DurationMinute { get; set; }

    public TimeSpan? ScheduleDuration { get; set; }

    public double UnitPriceMultiplier { get; set; }

    public Guid Uid { get; set; }
}
