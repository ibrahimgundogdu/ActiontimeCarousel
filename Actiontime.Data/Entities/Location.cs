using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Location
{
    public int Id { get; set; }

    public short OurCompanyId { get; set; }

    public string? LocationCode { get; set; }

    public short LocationTypeId { get; set; }

    public string LocationName { get; set; } = null!;

    public string? LocationTypeName { get; set; }

    public string? Description { get; set; }

    public short CountryId { get; set; }

    public short StateId { get; set; }

    public short CityId { get; set; }

    public string? Address { get; set; }

    public short Timezone { get; set; }

    public DateOnly? LocalDate { get; set; }

    public DateTime? LocalDateTime { get; set; }

    public string? MapUrl { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public double? TaxRate { get; set; }

    public short? PriceCatId { get; set; }

    public string? Currency { get; set; }

    public Guid? LocationUid { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }
}
