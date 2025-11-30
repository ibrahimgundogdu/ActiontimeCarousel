using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OrderRowStatus
{
    public int Id { get; set; }

    public string? StatusName { get; set; }

    public string? StatusColor { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
