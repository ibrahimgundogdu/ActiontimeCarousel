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
        public string SSK { get; set; }
        public string GV { get; set; }
        public string DV { get; set; }

    }
}