using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class SalaryEarn
    {
        public int ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        //public double? UnitPrice { get; set; }
        public double? QuantityHour { get; set; }
        //public double TotalAmount { get; set; }
        public int LocationID { get; set; }
        public int OurCompanyID { get; set; }
        public int? EmployeeID { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public int? EnvironmentID { get; set; }
        public int? TimeZone { get; set; }
        public Guid UID { get; set; }
        //public double? SystemUnitPrice { get; set; }
        //public double? SystemQuantityHour { get; set; }
        //public double? SystemTotalAmount { get; set; }
        public int? CategoryID { get; set; }
    }
}