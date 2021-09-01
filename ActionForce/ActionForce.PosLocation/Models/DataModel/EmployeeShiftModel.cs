using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class EmployeeShiftModel
    {
        public int EmployeeID { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime? ScheduleDateStart { get; set; }
        public DateTime? ScheduleDateEnd { get; set; }
        public TimeSpan? ScheduleDuration { get; set; }
        public DateTime? ShiftDateStart { get; set; }
        public DateTime? ShiftDateEnd { get; set; }
        public TimeSpan? ShiftDuration { get; set; }
    }
}