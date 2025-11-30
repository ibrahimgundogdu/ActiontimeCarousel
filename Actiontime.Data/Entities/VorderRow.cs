using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class VorderRow
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public DateTime Date { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public string? TicketNumber { get; set; }

    public int Quantity { get; set; }

    public int? Unit { get; set; }

    public int? Duration { get; set; }

    public decimal? Price { get; set; }

    public decimal? Amount { get; set; }

    public double? TaxRate { get; set; }

    public decimal? Total { get; set; }

    public Guid? Uid { get; set; }

    public string? StatusName { get; set; }

    public string? TicketTypeName { get; set; }

    public string? Sign { get; set; }

    public string? MethodName { get; set; }

    public int? RowStatusId { get; set; }

    public string? TicketDuration { get; set; }

    public string? TripDuration { get; set; }

    public int? TripDurationSecond { get; set; }

    public DateTime? DateKey { get; set; }
}
