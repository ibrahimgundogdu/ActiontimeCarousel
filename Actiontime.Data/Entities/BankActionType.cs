using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class BankActionType
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public double? Multiply { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
