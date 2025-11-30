using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class DayResultState
{
    public int Id { get; set; }

    public string? StateName { get; set; }

    public string? SortBy { get; set; }
}
