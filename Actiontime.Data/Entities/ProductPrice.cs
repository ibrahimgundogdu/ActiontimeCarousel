using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class ProductPrice
{
    public int Id { get; set; }

    public int OurCompanyId { get; set; }

    public int PriceCategoryId { get; set; }

    public int ProductId { get; set; }

    public double Price { get; set; }

    public string Currency { get; set; } = null!;

    public int? Quantity { get; set; }

    public int? Duration { get; set; }

    public double? MasterPrice { get; set; }

    public double? PromoPrice { get; set; }

    public double TotalPrice { get; set; }

    public string? ProductName { get; set; }

    public double? TaxRate { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? PriceCategoryName { get; set; }
}
