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
        public IEnumerable<VLocationSchedule> VLocationSchedule { get; set; }

        public IEnumerable<VSchedule> EmployeeSchedule { get; set; }
        public IEnumerable<EmployeeLocation> EmployeeLocations { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
        public IEnumerable<DateList> CurrentWeek { get; set; }
        public IEnumerable<DateList> NextWeek { get; set; }
        public DateList CurrentDate { get; set; }

        public FilterModel Filters { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Employee CurrentEmployee { get; set; }
        public Result<Schedule> EmployeeResult { get; set; }
        public Result<LocationSchedule> LocationResult { get; set; }
        public Result<Schedule> ResultMessage { get; set; }

        public IEnumerable<DateList> WeekList { get; set; }
        public DateList FirstWeekDay { get; set; }
        public DateList LastWeekDay { get; set; }

        public IEnumerable<Location> ScheduledLocationList { get; set; }
        public IEnumerable<Location> NonScheduledLocationList { get; set; }


        public string calendarEvents { get; set; }
        public string WeekCode { get; set; }
        public string NextWeekCode { get; set; }
        public string PrevWeekCode { get; set; }
        public int? LocationID { get; set; }

        public int SuccessCount { get; set; }
        public int WaitingCount { get; set; }
        public int TotalCount { get; set; }
        public int SuccessRate { get; set; }


    }
}