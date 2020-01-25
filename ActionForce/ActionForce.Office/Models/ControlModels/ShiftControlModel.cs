using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ShiftControlModel : LayoutControlModel
    {
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<Employee> Employees { get; set; }

        public IEnumerable<LocationShift> LocationShifts { get; set; }
        public IEnumerable<LocationSchedule> LocationSchedules { get; set; }

        public LocationShift LocationShift { get; set; }
        public LocationSchedule LocationSchedule { get; set; }
        public Schedule EmployeeSchedule { get; set; }


        public IEnumerable<EmployeeShift> EmployeeShifts { get; set; }
        public EmployeeShift EmployeeShift { get; set; }
        public IEnumerable<EmployeeShift> EmployeeBreaks { get; set; }
        public IEnumerable<Schedule> EmployeeSchedules { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public Location CurrentLocation { get; set; }
        public Employee CurrentEmployee { get; set; }

        public DateList CurrentDate { get; set; }
        public Result<EmployeeShift> Result { get; set; }


        public string TodayDateCode { get; set; }
        public string CurrentDateCode { get; set; }
        public string NextDateCode { get; set; }
        public string PrevDateCode { get; set; }

        public string WeekCode { get; set; }
        public string NextWeekCode { get; set; }
        public string PrevWeekCode { get; set; }

        public List<DateList> WeekList { get; set; }
        public DateList FirstWeekDay { get; set; }
        public DateList LastWeekDay { get; set; }
    }
}