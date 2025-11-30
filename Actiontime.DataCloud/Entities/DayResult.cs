using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class DayResult
{
    public long Id { get; set; }

    public int LocationId { get; set; }

    public DateTime Date { get; set; }

    public int StateId { get; set; }

    public int? StatusId { get; set; }

    public int? EnvironmentId { get; set; }

    public string? Description { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public bool? IsMobile { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public string? UpdateIp { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? RecordIp { get; set; }

    public bool? IsActive { get; set; }

    public Guid? Uid { get; set; }
}
