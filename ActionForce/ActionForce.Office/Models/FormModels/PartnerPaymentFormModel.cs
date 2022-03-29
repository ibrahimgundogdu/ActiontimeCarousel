using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnerPaymentFormModel
    {
        public long DocumentPaymentID { get; set; }
        public DateTime DocumentDate { get; set; }
        public int PartnershipID { get; set; }
        public string ExpensePeriodCode { get; set; }
        public short PayMethodID { get; set; }
        public string PaymentAmount { get; set; }
        public string DocumentSource { get; set; }
        public string PaymentDescription { get; set; }
        public string IsActive { get; set; }
        public Guid UID { get; set; }

    }
}