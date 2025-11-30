using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class LocationSchedule
{
    public long Id { get; set; }

    public int LocationId { get; set; }

    public short TimeZone { get; set; }

    public string ScheduleWeek { get; set; } = null!;

    public DateTime ScheduleStart { get; set; }

    public DateTime? ScheduleEnd { get; set; }

    public DateTime? ScheduleDate { get; set; }

    public int? DurationMinute { get; set; }

    public TimeSpan? ScheduleDuration { get; set; }

    public double UnitPriceMultiplier { get; set; }

    public Guid Uid { get; set; }
}
