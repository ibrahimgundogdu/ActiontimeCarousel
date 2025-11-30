using System;
using System.Collections.Generic;

namespace Actiontime.Data.Entities;

public partial class DatePeriod
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public int Week { get; set; }

    public int? Day { get; set; }

    public int? DayOfYear { get; set; }

    public string? MonthName { get; set; }

    public string? DayName { get; set; }

    public int? Quarter { get; set; }

    public string? Description { get; set; }

    public int WeekYear { get; set; }

    public int WeekNumber { get; set; }

    public string? PeriodNumber { get; set; }
}
