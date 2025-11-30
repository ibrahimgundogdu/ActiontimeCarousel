using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class TicketSaleRow
{
    public long Id { get; set; }

    public long? ParentId { get; set; }

    public long SaleId { get; set; }

    public int? StatusId { get; set; }

    public DateTime Date { get; set; }

    public DateTime? DateKey { get; set; }

    public TimeSpan? TimeKey { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public bool? IsSale { get; set; }

    public long? TicketTripId { get; set; }

    public int? PaymethodId { get; set; }

    public bool? UseImmediately { get; set; }

    public int? PriceCategoryId { get; set; }

    public int TicketTypeId { get; set; }

    public string? TicketNumber { get; set; }

    public int Quantity { get; set; }

    public int? Unit { get; set; }

    public int? ExtraUnit { get; set; }

    public int PriceId { get; set; }

    public double Price { get; set; }

    public double? Discount { get; set; }

    public double? ExtraPrice { get; set; }

    public double? PrePaid { get; set; }

    public double? Total { get; set; }

    public string Currency { get; set; } = null!;

    public double? TaxRate { get; set; }

    public int? PromotionId { get; set; }

    public bool? IsPromotion { get; set; }

    public bool? IsExchangable { get; set; }

    public string? DeviceId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerData { get; set; }

    public string? Description { get; set; }

    public int? AnimalCostumeTypeId { get; set; }

    public int? MallMotoColorId { get; set; }

    public Guid? Uid { get; set; }

    public int? LocalRowId { get; set; }

    public int? ProductId { get; set; }

    public int? TicketProductId { get; set; }

    public int? CardReaderId { get; set; }

    public string? CardNumber { get; set; }

    public int? MasterCredit { get; set; }

    public int? PromoCredit { get; set; }

    public int? TotalCredit { get; set; }
}
