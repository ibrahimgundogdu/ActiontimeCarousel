using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Bank
{
    public int Id { get; set; }

    public int? OurCompanyId { get; set; }

    public string? Code { get; set; }

    public int? Eftcode { get; set; }

    public string? Name { get; set; }

    public string? ShortName { get; set; }

    public bool? Individual { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
