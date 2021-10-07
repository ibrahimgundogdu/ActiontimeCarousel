using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation.Models.DataModel
{
    public class EnvelopeDataModel
    {
        public Location Location { get; set; }
        public Employee Employee { get; set; }
        public DateList ShiftDate { get; set; }
        public DayResult DayResult { get; set; }
        public List<EnvelopeCheck> EnvelopeCheckList { get; set; }
        public LocationSchedule LocationSchedule { get; set; }
        public LocationShift LocationShift { get; set; }
       // public EmployeeSchedule EmployeeSchedule { get; set; }

    }
}