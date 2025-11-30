using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Order
{
    public int Id { get; set; }

    public short OrderStatusId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public short OrderTypeId { get; set; }

    public DateTime Date { get; set; }

    public short OurCompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public int? CustomerId { get; set; }

    public int? PartnerId { get; set; }

    public int? PartnerUserId { get; set; }

    public int? TourGroupId { get; set; }

    public short? PosTerminalId { get; set; }

    public short? KioskTerminalId { get; set; }

    public short? PaymentTerminalId { get; set; }

    /// <summary>
    /// 1 location on app, 2 office, 3 diğer
    /// </summary>
    public short? SaleChannelD { get; set; }

    public short PriceCategoryId { get; set; }

    public short? CardReaderId { get; set; }

    /// <summary>
    /// iptal veya iade durumlarında durum kodu seçmek için
    /// </summary>
    public short? ReasonId { get; set; }

    public short? PosStatusId { get; set; }

    public short? EnvironmentId { get; set; }

    public string? Description { get; set; }

    public double? Amount { get; set; }

    public double? TaxAmount { get; set; }

    public double? TotalAmount { get; set; }

    public string? Currency { get; set; }

    public string? CardNumber { get; set; }

    public Guid Uid { get; set; }

    public DateTime? RecordDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? SyncDate { get; set; }

    public bool? SendPaymentTerminal { get; set; }

    public bool? IsPaymentCompleted { get; set; }

    public bool? IsActive { get; set; }

    public Guid? Token { get; set; }

    public int? PrintCount { get; set; }

    public string? ReceiptNumber { get; set; }

    public int? TicketCount { get; set; }
}
