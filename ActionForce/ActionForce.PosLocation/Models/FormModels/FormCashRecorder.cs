using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class FormCashRecorder
    {
        public Guid? UID { get; set; }
        public string CashAmount { get; set; }
        public string CreditAmount { get; set; }
        public string NetAmount { get; set; }
        public string TotalAmount { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ReceiptTime { get; set; }
        public int? IsActive { get; set; }
        public HttpPostedFileBase ReceiptFile { get; set; }
    }
}