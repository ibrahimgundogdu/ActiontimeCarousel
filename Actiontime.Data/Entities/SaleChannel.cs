using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class SaleChannel
{
    public byte Id { get; set; }

    public string? SaleChannelName { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
