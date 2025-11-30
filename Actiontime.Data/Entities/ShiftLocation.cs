using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class ShiftLocation
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public int? EmployeeIdfinish { get; set; }

    public DateTime ShiftDate { get; set; }

    public TimeSpan? ShiftStart { get; set; }

    public TimeSpan? ShiftFinish { get; set; }

    public DateTime? ShiftDateStart { get; set; }

    public DateTime? ShiftDateFinish { get; set; }

    public string? Duration { get; set; }

    public int? DurationMinute { get; set; }

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

    public int? EnvironmentId { get; set; }

    public int? CloseEnvironmentId { get; set; }

    public TimeSpan? ShiftDuration { get; set; }
}
