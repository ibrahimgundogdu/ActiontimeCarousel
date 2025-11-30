using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class PriceCategory
{
    public short Id { get; set; }

    public short OurCompanyId { get; set; }

    public short? TicketTypeId { get; set; }

    public string? CategoryCode { get; set; }

    public string? CategoryName { get; set; }

    public DateTime? RecordDate { get; set; }

    public string? SortBy { get; set; }

    public bool? IsMaster { get; set; }

    public bool? IsActive { get; set; }
}
