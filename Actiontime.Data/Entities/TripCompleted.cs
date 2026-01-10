using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class TripCompleted
{
    public long Id { get; set; }

    public long? ConfirmId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public string? TicketNumber { get; set; }

    public string? ReaderSerialNumber { get; set; }

    public int? PartId { get; set; }

    public DateOnly? TripDate { get; set; }

    public DateTime? TripStart { get; set; }

    public DateTime? TripCancel { get; set; }

    public DateTime? TripEnd { get; set; }

    public int? UnitDuration { get; set; }

    public TimeOnly? TripDuration { get; set; }

    public int? TripDurationSecond { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public Guid? Uid { get; set; }
}
