using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class InspectionItemImage
{
    public int Id { get; set; }

    public int? InspectionItemId { get; set; }

    public string? ImageName { get; set; }

    public string? SortBy { get; set; }
}
