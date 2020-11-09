using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class EmployeeShiftLocationModel
    {
        public int EmployeeID { get; set; }
        public DateTime ShiftDate { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}