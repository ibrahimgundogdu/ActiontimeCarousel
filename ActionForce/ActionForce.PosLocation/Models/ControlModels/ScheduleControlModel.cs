using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class ScheduleControlModel : LayoutControlModel
    {
        public List<DateList> DateList { get; set; }
        public List<LocationSchedule> LocationSchedules { get; set; }
        public List<Schedule> EmployeeSchedules { get; set; }
        public List<DataEmployee> Employees { get; set; }
       


    }
}