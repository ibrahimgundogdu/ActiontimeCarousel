using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class FormSalaryPay
    {
        public int EmployeeID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public DateTime DocumentDate { get; set; }
        public Guid UID { get; set; }
        public int? IsActive { get; set; }
    }
}