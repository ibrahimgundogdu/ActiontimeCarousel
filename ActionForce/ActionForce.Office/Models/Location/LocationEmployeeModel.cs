using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class LocationEmployeeModel
    {
        public int EmployeeID { get; set; }
        public Guid EmployeeUID { get; set; }
        public string FullName { get; set; }
        public string PositionName { get; set; }
        public bool Active { get; set; }
    }
}