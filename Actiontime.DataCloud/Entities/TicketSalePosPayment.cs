using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class TicketSalePosPayment
{
    public long Id { get; set; }

    public long SaleId { get; set; }

    public bool? FromPosTerminal { get; set; }

    public int PaymentType { get; set; }

    public string? PaymentSubType { get; set; }

    public int? NumberOfInstallment { get; set; }

    public double PaymentAmount { get; set; }

    public string? PaymentDesc { get; set; }

    public int PaymentCurrency { get; set; }

    public string? PaymentInfo { get; set; }

    public string? PaymentDateTime { get; set; }

    public DateOnly PaymentDate { get; set; }

    public TimeOnly PaymentTime { get; set; }

    public short? BankBkmid { get; set; }

    public string? BatchNumber { get; set; }

    public string? StanNumber { get; set; }

    public string? MerchantId { get; set; }

    public string? TerminalId { get; set; }

    public string ReferenceNumber { get; set; } = null!;

    public string? AuthorizationCode { get; set; }

    public string? MaskedPan { get; set; }

    public DateTime? RecordDate { get; set; }

    public string Currency { get; set; } = null!;
}
