using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class Location
{
    public int LocationId { get; set; }

    public string? LocationCode { get; set; }

    public int? LocationTypeId { get; set; }

    public string? LocationName { get; set; }

    public string? LocationNameSearch { get; set; }

    public string? Description { get; set; }

    public int? CountryId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? State { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public int? Timezone { get; set; }

    public string? MapUrl { get; set; }

    public int OurCompanyId { get; set; }

    public string? ImageFile { get; set; }

    public bool? IsHaveOperator { get; set; }

    public bool? UseCardSysteme { get; set; }

    public string? Ip { get; set; }

    public string? EnforcedWarning { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }

    public string? Currency { get; set; }

    public double? TaxRate { get; set; }

    public int? PriceCatId { get; set; }

    public int? ProductPriceCatId { get; set; }

    public string? Weight { get; set; }

    public string? Distance { get; set; }

    public DateTime? LocalDate { get; set; }

    public DateTime? LocalDateTime { get; set; }

    public string? LocationFullName { get; set; }

    public int? MallId { get; set; }

    public int? PosaccountId { get; set; }

    public Guid? LocationUid { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public string? RecordIp { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public string? UpdateIp { get; set; }

    public bool? ProfitCenter { get; set; }

    public bool? ExpenseCenter { get; set; }

    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }
}
