using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ComputeControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public IEnumerable<Location> Locations { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
        public IEnumerable<DateList> DateLists { get; set; }
        public int WeekYear { get; set; }
        public int WeekNumber { get; set; }
    }
}