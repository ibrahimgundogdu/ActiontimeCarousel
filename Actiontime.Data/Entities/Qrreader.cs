using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Qrreader
{
    public int Id { get; set; }

    public int? OurCompanyId { get; set; }

    public int? LocationId { get; set; }

    public int? LocationTypeId { get; set; }

    public int? QrreaderTypeId { get; set; }

    public int? LocationPartId { get; set; }

    public string? PartName { get; set; }

    public string? PartGroupName { get; set; }

    public string? SerialNumber { get; set; }

    public string? Macaddress { get; set; }

    public string? Version { get; set; }

    public string? Ipaddress { get; set; }

    public int? DurationTime { get; set; }

    public int? TriggerCount { get; set; }

    public int? TriggerTime { get; set; }

    public Guid? Uid { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? IsActive { get; set; }
}
