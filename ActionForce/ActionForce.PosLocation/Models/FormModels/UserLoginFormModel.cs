using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class UserLoginFormModel
    {
        public int EmployeeID { get; set; }
        public int LocationID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

    }
}