using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class SalaryPayment
    {
        public long ID { get; set; }
        public Guid UID { get; set; }
        public string DocumentNumber { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
    }
}