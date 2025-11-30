using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class LocationPartial
{
    public int Id { get; set; }

    public int? PartialId { get; set; }

    public short OurCompanyId { get; set; }

    public int LocationId { get; set; }

    public short PartialTypeId { get; set; }

    public short? Number { get; set; }

    public string? Code { get; set; }

    public string? PartName { get; set; }

    public string? Direction { get; set; }

    public bool? IsActive { get; set; }
}
