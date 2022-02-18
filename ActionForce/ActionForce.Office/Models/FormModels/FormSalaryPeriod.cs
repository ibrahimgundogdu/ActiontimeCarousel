using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormSalaryPeriod
    {
        public long ID { get; set; }
        public int SalaryPeriodID { get; set; }
        public int EmployeeID { get; set; }

        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string FoodCard { get; set; }
        public string IBAN { get; set; }
        public string BankName { get; set; }
        public string SGKBranch { get; set; }
        public string LocationName { get; set; }


        public string SalaryTotal { get; set; }
        public string PermitTotal { get; set; }
        public string ExtraShiftTotal { get; set; }
        public string PremiumTotal { get; set; }
        public string FormalTotal { get; set; }
        public string OtherTotal { get; set; }

        public string PrePaymentAmount { get; set; }
        public string SalaryCutAmount { get; set; }
        public string PermitPaymentAmount { get; set; }
        public string ExtraShiftPaymentAmount { get; set; }
        public string PremiumPaymentAmount { get; set; }
        public string FormalPaymentAmount { get; set; }
        public string OtherPaymentAmount { get; set; }

        public string BankPaymentAmount { get; set; }
        public string ManuelPaymentAmount { get; set; }
        public string TransferBalance { get; set; }

        public string NetCost { get; set; }
        public string Tahakkuk { get; set; }
        public string SSK { get; set; }
        public string GV { get; set; }
        public string DV { get; set; }
        public string Kidem { get; set; }
        public string Ihbar { get; set; }
        public string Permit { get; set; }

        public string TesvikNumber { get; set; }
        public string TesvikDiscount { get; set; }
        public string SSKDayCount { get; set; }


    }
}