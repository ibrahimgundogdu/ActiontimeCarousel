using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class Location1
{
    public int LocationId { get; set; }

    public string? LocationCode { get; set; }

    public int? LocationTypeId { get; set; }

    public string? LocationName { get; set; }

    public string? LocationNameSearch { get; set; }

    public string? Description { get; set; }

    public string? State { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public int? Timezone { get; set; }

    public string? MapUrl { get; set; }

    public int OurCompanyId { get; set; }

    public string? ImageFile { get; set; }

    public bool? IsHaveOperator { get; set; }

    public string? Ip { get; set; }

    public string? EnforcedWarning { get; set; }

    public string? SortBy { get; set; }

    public bool? IsActive { get; set; }

    public string? Currency { get; set; }

    public int? PriceCatId { get; set; }

    public string? Weight { get; set; }

    public string? Distance { get; set; }

    public DateTime? LocalDateTime { get; set; }

    public DateOnly? LocalDate { get; set; }

    public int? DateId { get; set; }

    public DateOnly? DateKey { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public int? Week { get; set; }

    public int? Day { get; set; }

    public int? DayOfYear { get; set; }

    public string? MonthName { get; set; }

    public string? DayName { get; set; }

    public string? MonthNameTr { get; set; }

    public string? DayNameTr { get; set; }

    public int? Quarter { get; set; }

    public int? WeekYear { get; set; }

    public int? WeekNumber { get; set; }

    public DateOnly? ShiftDate { get; set; }

    public TimeOnly? ShiftStart { get; set; }

    public TimeOnly? ShiftFinish { get; set; }

    public string? Duration { get; set; }

    public int? Status { get; set; }

    public Guid? LocationUid { get; set; }
}
