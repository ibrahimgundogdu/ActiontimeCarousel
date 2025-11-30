using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class PayMethod
{
    public int Id { get; set; }

    public string? MethodName { get; set; }

    public bool? IsActive { get; set; }
}
