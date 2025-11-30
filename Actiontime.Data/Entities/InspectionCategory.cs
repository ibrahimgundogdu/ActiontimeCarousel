using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class InspectionCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
