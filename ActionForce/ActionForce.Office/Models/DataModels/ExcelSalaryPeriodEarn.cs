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
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string FoodCard { get; set; }
        public string IBAN { get; set; }
        public string BankName { get; set; }
        public short? SalaryPaymentTypeID { get; set; }
        public string SGKBranch { get; set; }
        public string LocationName { get; set; }

        public double? SalaryTotal { get; set; }
        public double? PermitTotal { get; set; }
        public double? ExtraShiftTotal { get; set; }
        public double? PremiumTotal { get; set; }
        public double? FormalTotal { get; set; }
        public double? OtherTotal { get; set; }
        public double? TotalProgress { get; set; }
        public double? PrePaymentAmount { get; set; }
        public double? SalaryCutAmount { get; set; }
        public double? PermitPaymentAmount { get; set; }
        public double? ExtraShiftPaymentAmount { get; set; }
        public double? PremiumPaymentAmount { get; set; }
        public double? FormalPaymentAmount { get; set; }
        public double? OtherPaymentAmount { get; set; }
        public double? TotalPaymentAmount { get; set; }
        public double? TotalBalance { get; set; }
        public double? BankPaymentAmount { get; set; }
        public double? ManuelPaymentAmount { get; set; }
        public double? TransferBalance { get; set; }
        public double? GrossBalance { get; set; }
        public double? FoodCardTotal { get; set; }
        public double? FoodCardPaymentAmount { get; set; }
        public double? FoodcardBalance { get; set; }
        public double? NetCost { get; set; }
        public double? Tahakkuk { get; set; }
        public double? SSK { get; set; }
        public double? GV { get; set; }
        public double? DV { get; set; }
        public double? Kidem { get; set; }
        public double? Ihbar { get; set; }
        public double? Permit { get; set; }
        public double? TotalCost { get; set; }
        public string TesvikNumber { get; set; }
        public double? TesvikDiscount { get; set; }
        public int? SSKDayCount { get; set; }
        public string Currency { get; set; }

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