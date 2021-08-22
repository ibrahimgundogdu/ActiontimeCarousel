using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class EmployeeScheduleModel
    {
        public int EmployeeID { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}