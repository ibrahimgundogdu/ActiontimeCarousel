using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class DataEmployee
    {
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeFullname { get; set; }
        public bool IsScheduled { get; set; }
        public string PhotoFile { get; set; }
    }
}