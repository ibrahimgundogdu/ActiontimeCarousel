using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class DrawerDevice
{
    public int Id { get; set; }

    public int OurCompanyId { get; set; }

    public int LocationId { get; set; }

    public string PartName { get; set; } = null!;

    public string SerialNumber { get; set; } = null!;

    public string Version { get; set; } = null!;

    public string Ipaddress { get; set; } = null!;

    public Guid Uid { get; set; }

    public DateTime DateRecord { get; set; }

    public bool IsActive { get; set; }
}
