using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class VorderItem
{
    public long Id { get; set; }

    public int OrderId { get; set; }

    public int? LocationId { get; set; }

    public int? EmployeeId { get; set; }

    public short? PriceCategoryId { get; set; }

    public short PriceId { get; set; }

    public int? ProductId { get; set; }

    public DateTime Date { get; set; }

    public int Quantity { get; set; }

    public int? Unit { get; set; }

    public double Price { get; set; }

    public double? TaxRate { get; set; }

    public double? Tax { get; set; }

    public double? Discount { get; set; }

    public double Total { get; set; }

    public string Currency { get; set; } = null!;

    public int? PromotionId { get; set; }

    public bool? IsPromotion { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public double? MasterPrice { get; set; }

    public double? PromoPrice { get; set; }

    public double? TotalPrice { get; set; }

    public Guid? Token { get; set; }
}
