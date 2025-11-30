using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class QrreaderParameter
{
    public short Id { get; set; }

    public int? LocationId { get; set; }

    public int? QrreaderTypeId { get; set; }

    public int? DurationTime { get; set; }

    public int? TriggerCount { get; set; }

    public int? TriggerTime { get; set; }
}
