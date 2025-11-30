using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class ShiftEmployee
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public int? LocationId { get; set; }

    public DateTime? ShiftDate { get; set; }

    public TimeSpan? ShiftStart { get; set; }

    public TimeSpan? ShiftEnd { get; set; }

    public string? Duration { get; set; }

    public bool? IsWorkTime { get; set; }

    public bool? IsBreakTime { get; set; }

    public DateTime? ShiftDateStart { get; set; }

    public DateTime? ShiftDateEnd { get; set; }

    public TimeSpan? ShiftDuration { get; set; }

    public int? DurationMinute { get; set; }

    public TimeSpan? BreakStart { get; set; }

    public TimeSpan? BreakEnd { get; set; }

    public TimeSpan? BreakDuration { get; set; }

    public DateTime? BreakDateStart { get; set; }

    public DateTime? BreakDateEnd { get; set; }

    public int? BreakDurationMinute { get; set; }

    public double? LatitudeStart { get; set; }

    public double? LongitudeStart { get; set; }

    public bool? FromMobileStart { get; set; }

    public double? LatitudeFinish { get; set; }

    public double? LongitudeFinish { get; set; }

    public bool? FromMobileFinish { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? BreakTypeId { get; set; }

    public int? EnvironmentId { get; set; }

    public int? CloseEnvironmentId { get; set; }
}
