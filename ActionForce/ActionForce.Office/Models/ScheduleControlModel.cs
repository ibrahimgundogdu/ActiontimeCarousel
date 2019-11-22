using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ScheduleControlModel : LayoutControlModel
    {
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<LocationSchedule> LocationSchedule { get; set; }
        public IEnumerable<Schedule> EmployeeSchedule { get; set; }
        public IEnumerable<DateList> CurrentWeek { get; set; }
        public IEnumerable<DateList> NextWeek { get; set; }
        public DateList CurrentDate { get; set; }

        public FilterModel Filters { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<Schedule> EmployeeResult { get; set; }
        public Result<LocationSchedule> LocationResult { get; set; }
        public Result<Schedule> ResultMessage { get; set; }

        public string calendarEvents { get; set; }

    }
}