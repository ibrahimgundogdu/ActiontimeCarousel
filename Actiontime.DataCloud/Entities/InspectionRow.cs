using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class InspectionRow
{
    public int Id { get; set; }

    public int InspectionId { get; set; }

    public int InspectionItemId { get; set; }

    public int InspectionCategoryId { get; set; }

    public int LocationPartId { get; set; }

    public string? LanguageCode { get; set; }

    public string? InspectionItemName { get; set; }

    public string? InspectionValue { get; set; }

    public string? EstimatedValue { get; set; }

    public string? Description { get; set; }

    public int? InspectorId { get; set; }

    public DateTime? InpectionDate { get; set; }
}
