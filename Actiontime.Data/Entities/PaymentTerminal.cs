using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class PaymentTerminal
{
    public short Id { get; set; }

    public short? OurCompayId { get; set; }

    public int? LocationId { get; set; }

    public short PaymentTerminalNumber { get; set; }

    public short? PosTerminalId { get; set; }

    public short? KioskTerminalId { get; set; }

    public string? BrandName { get; set; }

    public string? ModelName { get; set; }

    public string? SerialNumber { get; set; }

    public Guid? Uid { get; set; }

    public DateTime? RecordDate { get; set; }

    public bool? IsMaster { get; set; }

    public bool? IsActive { get; set; }
}
