using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class LocationScheduleEdit
    {
        public string isActive { get; set; }
        public string dateKey { get; set; }
        public int? locationID { get; set; }
        public int? dateID { get; set; }
        public long? scheduleID { get; set; }
        public string ShiftBeginDate { get; set; }
        public string ShiftBeginTime { get; set; }
        public string ShiftEndDate { get; set; }
        public string ShiftEndTime { get; set; }
        public string weekCode { get; set; }
    }

    public class EmployeeScheduleEdit
    {
        public string isActive { get; set; }
        public string dateKey { get; set; }
        public int? locationID { get; set; }
        public int? employeeID { get; set; }
        public int? dateID { get; set; }
        public long? scheduleID { get; set; }
        public string ShiftBeginDate { get; set; }
        public string ShiftBeginTime { get; set; }
        public string ShiftEndDate { get; set; }
        public string ShiftEndTime { get; set; }
        public string weekCode { get; set; }
    }
}