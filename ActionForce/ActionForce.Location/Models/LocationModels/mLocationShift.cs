using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class mLocationShift
    {
        public int? LocationID { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public int? DurationMinute { get; set; }
    }
}