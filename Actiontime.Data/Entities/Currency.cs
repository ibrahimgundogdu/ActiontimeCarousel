using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Currency
{
    public short Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Name { get; set; }

    public string? Sign { get; set; }
}
