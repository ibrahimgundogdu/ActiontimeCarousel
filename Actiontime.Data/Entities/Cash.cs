using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Cash
{
    public int Id { get; set; }

    public short? OurCompanyId { get; set; }

    public int LocationId { get; set; }

    public short? CashTypeId { get; set; }

    public string? CashName { get; set; }

    public double? BlockedAmount { get; set; }

    public string? Currency { get; set; }

    public bool? IsMaster { get; set; }

    public string? SortBy { get; set; }
}
