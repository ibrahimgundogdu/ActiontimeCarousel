using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Inspection
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public short InspectionTypeId { get; set; }

    public int InspectorId { get; set; }

    public DateTime InspectionDate { get; set; }

    public DateTime? DateBegin { get; set; }

    public DateTime? DateEnd { get; set; }

    public string? Description { get; set; }

    public string? LanguageCode { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public Guid? Uid { get; set; }
}
