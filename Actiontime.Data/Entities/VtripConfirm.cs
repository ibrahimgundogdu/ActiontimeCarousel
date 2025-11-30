using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class VtripConfirm
{
    public long? ConfirmId { get; set; }

    public long TripId { get; set; }

    public Guid? ConfirmNumber { get; set; }

    public long? SaleOrderId { get; set; }

    public long? SaleOrderRowId { get; set; }

    public int? EmployeeId { get; set; }

    public int? LocationId { get; set; }

    public int? LocationPartId { get; set; }

    public string? ReaderSerialNumber { get; set; }

    public string? TicketNumber { get; set; }

    public int? UnitDuration { get; set; }

    public DateTime? RecordDate { get; set; }

    public bool? IsApproved { get; set; }

    public int? PartialId { get; set; }

    public string? PartName { get; set; }

    public DateTime? LocalDateTime { get; set; }

    public string? LocationName { get; set; }

    public short? Timezone { get; set; }

    public string? FullName { get; set; }

    public DateTime? TripStart { get; set; }

    public DateTime? TripEnd { get; set; }

    public TimeSpan? TripDuration { get; set; }

    public int? TripDurationSecond { get; set; }
}
