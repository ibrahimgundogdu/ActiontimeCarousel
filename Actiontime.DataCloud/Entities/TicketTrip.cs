using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class TicketTrip
{
    public long Id { get; set; }

    public long? ConfirmId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public int? TicketTypeId { get; set; }

    public string? TicketNumber { get; set; }

    public int? AnimalId { get; set; }

    public string? SerialNumber { get; set; }

    public int? PartId { get; set; }

    public string? PartNumber { get; set; }

    public DateOnly? TripDate { get; set; }

    public DateTime? TripStart { get; set; }

    public DateTime? TripCancel { get; set; }

    public DateTime? TripEnd { get; set; }

    public TimeOnly? TripDuration { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerPhone { get; set; }

    public string? IdentityCard { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? TripDurationSn { get; set; }
}
