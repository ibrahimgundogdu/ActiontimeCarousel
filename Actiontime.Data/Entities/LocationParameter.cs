using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class LocationParameter
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public DateOnly? DateStart { get; set; }

    public DateOnly? DateFinish { get; set; }

    public int TypeId { get; set; }

    public string? Description { get; set; }

    public double? Total { get; set; }

    public double? Rate { get; set; }

    public string? Currency { get; set; }

    public Guid? Uid { get; set; }
}
