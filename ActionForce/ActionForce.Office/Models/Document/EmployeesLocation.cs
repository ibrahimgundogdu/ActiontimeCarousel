using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office.Models.Document
{
    public class EmployeesLocation
    {
        public int EmployeeID { get; set; }
        public int LocationID { get; set; }
        public int PositionID { get; set; }
        public bool IsActive { get; set; }
        public bool IsMaster { get; set; }
    }
}