using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class LocationParameter
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public DateTime? DateStart { get; set; }

    public DateTime? DateFinish { get; set; }

    public int TypeId { get; set; }

    public string? Description { get; set; }

    public double? Total { get; set; }

    public double? Rate { get; set; }

    public string? Currency { get; set; }

    public Guid? Uid { get; set; }
}
