using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class Vaction
{
    public long Id { get; set; }

    public int? SourceId { get; set; }

    public string? SourceName { get; set; }

    public int? LocationId { get; set; }

    public int? ActionTypeId { get; set; }

    public DateTime? ActionDate { get; set; }

    public int? ProcessId { get; set; }

    public string? ProcessName { get; set; }

    public string ProcessType { get; set; } = null!;

    public double? Collection { get; set; }

    public double? Payment { get; set; }

    public double? Amount { get; set; }

    public string? Currency { get; set; }

    public DateTime? RecordDate { get; set; }

    public Guid? ProcessUid { get; set; }
}
