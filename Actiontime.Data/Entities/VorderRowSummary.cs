using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class VorderRowSummary
{
    public long Id { get; set; }

    public int? LocationId { get; set; }

    public DateOnly? DateKey { get; set; }

    public int? Quantity { get; set; }

    public int? Duration { get; set; }

    public decimal? Total { get; set; }

    public string? StatusName { get; set; }

    public string? TicketTypeName { get; set; }

    public string? MethodName { get; set; }

    public decimal? Price { get; set; }
}
