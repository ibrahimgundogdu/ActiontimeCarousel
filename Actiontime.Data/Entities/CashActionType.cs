using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class CashActionType
{
    public int Id { get; set; }

    public string? Module { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public double? Multiply { get; set; }

    public bool? IsMobile { get; set; }

    public string? MobileTag { get; set; }
}
