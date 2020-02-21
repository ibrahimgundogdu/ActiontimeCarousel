using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class NewPhone
    {
        public int EmployeeID { get; set; }
        public string CountryPhoneCode { get; set; }
        public string Mobile { get; set; }
        public int? PhoneType { get; set; }
        public string Description { get; set; }
        public string IsMaster { get; set; }
        public string IsActive { get; set; }
    }

    public class NewEmail
    {
        public int EmployeeID { get; set; }
        public string EMail { get; set; }
        public int? EmailType { get; set; }
        public string Description { get; set; }
        public string IsMaster { get; set; }
        public string IsActive { get; set; }
    }
}