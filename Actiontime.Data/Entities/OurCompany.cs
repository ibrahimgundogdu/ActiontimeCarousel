using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class OurCompany
{
    public short Id { get; set; }

    public string? AccountCode { get; set; }

    public string? CompanyName { get; set; }

    public string? Address { get; set; }

    public string? TaxOffice { get; set; }

    public string? TaxNumber { get; set; }

    public int? TimeZone { get; set; }

    public string? Currency { get; set; }

    public string? CurrencySign { get; set; }

    public string? Culture { get; set; }
}
