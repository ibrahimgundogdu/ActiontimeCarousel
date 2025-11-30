using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OrderType
{
    public short Id { get; set; }

    public string? SaleTypeName { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
