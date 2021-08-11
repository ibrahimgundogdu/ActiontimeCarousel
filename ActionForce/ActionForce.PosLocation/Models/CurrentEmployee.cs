using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class CurrentEmployee
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string FotoFile { get; set; }
        public Guid? Token { get; set; }
        public string Position { get; set; }
    }

}