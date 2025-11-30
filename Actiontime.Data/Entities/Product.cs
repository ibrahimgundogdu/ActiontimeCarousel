using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Product
{
    public int Id { get; set; }

    public short? OurCompanyId { get; set; }

    public short? ProductCategoryId { get; set; }

    public string? Sku { get; set; }

    public string? StockCode { get; set; }

    public string? Barcode { get; set; }

    public string? ProductName { get; set; }

    public string? Property { get; set; }

    public string? PropertyValue { get; set; }

    public short? UnitId { get; set; }

    public string? UnitCode { get; set; }

    public double? TaxRate { get; set; }

    public string? Image { get; set; }

    public Guid? Uid { get; set; }

    /// <summary>
    /// Sıfır stok ile çalışılsın mı
    /// </summary>
    public bool? AllowZeroStock { get; set; }

    /// <summary>
    /// Satışta mı
    /// </summary>
    public bool? AllowSale { get; set; }

    /// <summary>
    /// Stok Sayımı Yapılırmı
    /// </summary>
    public bool? IsEnvanter { get; set; }

    public bool? IsActive { get; set; }
}
