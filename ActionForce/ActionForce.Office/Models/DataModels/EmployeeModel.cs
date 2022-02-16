using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string IdentityNumber { get; set; }
        public string IBAN { get; set; }
        public string FoodCardNumber { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string BankBranchCode { get; set; }
        public string Currency { get; set; }
        public string MobilePhone { get; set; }
        public short SalaryPaymentTypeID { get; set; }
        public string SGKBranch { get; set; }
        public string LocationName { get; set; }


    }
}