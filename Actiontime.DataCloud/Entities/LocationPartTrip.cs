using System;
using System.Collections.Generic;

namespace Actiontime.DataCloud.Entities;

public partial class LocationPartTrip
{
    public long Id { get; set; }

    public Guid ConfirmNumber { get; set; }

    public string TicketNumber { get; set; } = null!;

    public int LocationId { get; set; }

    public int PartId { get; set; }

    public int PartSort { get; set; }

    public DateTime TripStart { get; set; }

    public DateTime? TripEnd { get; set; }

    public DateTime? LocalTime { get; set; }

    public short UnitDuration { get; set; }

    public int ElapsedDuration { get; set; }

    public string EmployeeName { get; set; } = null!;

    public short TimeZone { get; set; }

    public string Status { get; set; } = null!;
}
