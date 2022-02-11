using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ExcelSalaryPeriodEarn
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? SalaryTotal { get; set; }
        public double? PermitTotal { get; set; }
        public double? ExtraShiftTotal { get; set; }
        public double? PremiumTotal { get; set; }
        public double? FormalTotal { get; set; }
        public double? OtherTotal { get; set; }
        public double? FoodCardTotal { get; set; }
        public double? PrePaymentAmount { get; set; }
        public double? SalaryCutAmount { get; set; }
        public string Currency { get; set; }

    }
}