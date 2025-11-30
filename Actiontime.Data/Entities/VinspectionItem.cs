using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class VinspectionItem
{
    public int Id { get; set; }

    public int? InspectionTypeId { get; set; }

    public int? InspectionCatId { get; set; }

    public string? Number { get; set; }

    public string? ItemName { get; set; }

    public string? ItemNameTr { get; set; }

    public short? AnswerType { get; set; }

    public short? EstimatedTime { get; set; }

    public string? EstimatedAnswer { get; set; }

    public string? SortBy { get; set; }

    public bool? IsPart { get; set; }

    public bool? IsActive { get; set; }

    public string? CategoryName { get; set; }
}
