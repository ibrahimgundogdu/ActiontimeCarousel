using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OrderStatus
{
    public int Id { get; set; }

    public string? SaleStatusName { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
