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

        public double? PrePaymentAmount { get; set; }
        public double? SalaryCutAmount { get; set; }
        public double? PermitPaymentAmount { get; set; }
        public double? ExtraShiftPaymentAmount { get; set; }
        public double? PremiumPaymentAmount { get; set; }
        public double? FormalPaymentAmount { get; set; }
        public double? OtherPaymentAmount { get; set; }

        public string Currency { get; set; }

    }

    public class ExcelSalaryPeriodPayment
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? BankPaymentAmount { get; set; }
        public double? ManuelPaymentAmount { get; set; }
        public double? TransferBalance { get; set; }
        public string Currency { get; set; }

    }

 public class ExcelSalaryPeriodCost
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? NetCost { get; set; }
        public double? SSK { get; set; }
        public double? GV { get; set; }
        public double? DV { get; set; }

    }


    public class ExcelSalaryPeriodFoodEarn
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? FoodCardTotal { get; set; }
        public string Currency { get; set; }

    }

    public class ExcelSalaryPeriodFoodPayment
    {
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public double? FoodCardPaymentAmount { get; set; }
        public string Currency { get; set; }

    }
}