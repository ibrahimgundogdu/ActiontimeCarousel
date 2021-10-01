using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class PaymentFormModel
    {
        public long PosReceiptID { get; set; }
        public long OrderID { get; set; }
        public short? ReceiptNo { get; set; }
        public short? ZNo { get; set; }
        public short? EkuNo { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime ReceiptTime { get; set; }

        public string PaymentAmount { get; set; }
        public int? PosPaymentType { get; set; }
        public int? BankId { get; set; }
        public int? Installment { get; set; }
    }
}