using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class DayResultDocument
{
    public long Id { get; set; }

    public long ResultId { get; set; }

    public int DocumentTypeId { get; set; }

    public int LocationId { get; set; }

    public DateTime Date { get; set; }

    public int? EnvironmentId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public string? Description { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsActive { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public string? RecordIp { get; set; }

    public string? UpdateIp { get; set; }
}
