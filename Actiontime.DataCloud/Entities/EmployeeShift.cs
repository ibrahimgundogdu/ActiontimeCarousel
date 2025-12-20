using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class EmployeeShift
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public int? LocationId { get; set; }

    public DateOnly? ShiftDate { get; set; }

    public TimeOnly? ShiftStart { get; set; }

    public TimeOnly? ShiftEnd { get; set; }

    public string? Duration { get; set; }

    public bool? IsWorkTime { get; set; }

    public bool? IsBreakTime { get; set; }

    public DateTime? ShiftDateStart { get; set; }

    public DateTime? ShiftDateEnd { get; set; }

    public TimeOnly? ShiftDuration { get; set; }

    public int? DurationMinute { get; set; }

    public TimeOnly? BreakStart { get; set; }

    public TimeOnly? BreakEnd { get; set; }

    public TimeOnly? BreakDuration { get; set; }

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

    public Guid? Uid { get; set; }
}
