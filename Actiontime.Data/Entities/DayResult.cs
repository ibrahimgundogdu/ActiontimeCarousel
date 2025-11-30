using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class DayResult
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    public DateTime Date { get; set; }

    public int StateId { get; set; }

    public int? EnvironmentId { get; set; }

    public string? Description { get; set; }

    public string? PhotoFile { get; set; }

    public int? RecordEmployeeId { get; set; }

    public DateTime? RecordDate { get; set; }

    public int? UpdateEmployeeId { get; set; }

    public DateTime? UpdateDate { get; set; }

    public Guid? Uid { get; set; }
}
