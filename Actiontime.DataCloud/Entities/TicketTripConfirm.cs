using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class TicketTripConfirm
{
    public long Id { get; set; }

    public Guid? ConfirmNumber { get; set; }

    public long? TicketSaleId { get; set; }

    public long? TicketSaleRowId { get; set; }

    public int? LocationId { get; set; }

    public int? LocationPartId { get; set; }

    public int? QrreaderId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? ConfirmTime { get; set; }

    public string? TicketNumber { get; set; }

    public int? UnitDuration { get; set; }

    public DateTime? RecordDate { get; set; }

    public bool? IsApproved { get; set; }
}
