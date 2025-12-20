using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OrderRow
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? RowStatusId { get; set; }

    public DateTime Date { get; set; }

    public DateOnly? DateKey { get; set; }

    public TimeOnly? TimeKey { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public bool? IsSale { get; set; }

    public long? TicketTripId { get; set; }

    public int? PaymethodId { get; set; }

    public int? PriceCategoryId { get; set; }

    public int TicketTypeId { get; set; }

    public string? TicketNumber { get; set; }

    public int Quantity { get; set; }

    public int? Unit { get; set; }

    public int? ExtraUnit { get; set; }

    public int? Duration { get; set; }

    public int? ProductId { get; set; }

    public int PriceId { get; set; }

    public double Price { get; set; }

    public double? Discount { get; set; }

    public double? ExtraPrice { get; set; }

    public double? PrePaid { get; set; }

    public double? Amount { get; set; }

    public double? TaxRate { get; set; }

    public double? Total { get; set; }

    public string Currency { get; set; } = null!;

    public int? QrreaderId { get; set; }

    public string? DeviceId { get; set; }

    public int? PromotionId { get; set; }

    public bool? IsPromotion { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public DateTime? SyncDate { get; set; }

    public string? Description { get; set; }

    public Guid? Uid { get; set; }
}
