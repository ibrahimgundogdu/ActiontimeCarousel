using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OrderPosPayment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int PaymentType { get; set; }

    public double PaymentAmount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public string DocumentNumber { get; set; } = null!;

    public int? NumberOfInstallment { get; set; }

    public bool? FromPosTerminal { get; set; }

    public string? BatchNumber { get; set; }

    public string? MerchantId { get; set; }

    public string? TerminalId { get; set; }

    public string? AuthorizationCode { get; set; }

    public string? MaskedPan { get; set; }

    public DateTime? RecordDate { get; set; }

    public Guid? Uid { get; set; }
}
