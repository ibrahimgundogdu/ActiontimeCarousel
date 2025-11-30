using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class TripConfirm
{
    public long Id { get; set; }

    public Guid? ConfirmNumber { get; set; }

    public long? SaleOrderId { get; set; }

    public long? SaleOrderRowId { get; set; }

    public int? EmployeeId { get; set; }

    public int? LocationId { get; set; }

    public int? LocationPartId { get; set; }

    public string? ReaderSerialNumber { get; set; }

    public DateTime? ConfirmTime { get; set; }

    public string? TicketNumber { get; set; }

    public int? UnitDuration { get; set; }

    public DateTime? RecordDate { get; set; }

    public bool? IsApproved { get; set; }
}
