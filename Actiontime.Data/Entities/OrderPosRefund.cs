using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OrderPosRefund
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int RefundType { get; set; }

    public double RefundAmount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime RefoundDate { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public string? Description { get; set; }

    public bool? FromPosTerminal { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? RecordEmployeeId { get; set; }

    public Guid? Uid { get; set; }
}
