using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class TripRound
{
    public long Id { get; set; }

    public int LocationId { get; set; }

    public string RoundNumber { get; set; } = null!;

    public long RoundNumberInt { get; set; }

    public DateOnly RoundDate { get; set; }

    public DateTimeOffset? RoundStart { get; set; }

    public DateTimeOffset? RoundCancel { get; set; }

    public DateTimeOffset? RoundEnd { get; set; }

    public TimeOnly? TripDuration { get; set; }

    public int? TripDurationSecond { get; set; }

    public DateTimeOffset RecordDate { get; set; }

    public Guid Uid { get; set; }
}
