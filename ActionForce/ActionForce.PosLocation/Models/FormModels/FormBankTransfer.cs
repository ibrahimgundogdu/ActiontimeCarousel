using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class FormBankTransfer
    {
        public int BankAccountID { get; set; }
        public Guid? UID { get; set; }
        public string Amount { get; set; }
        public string Commission { get; set; }
        public string Currency { get; set; }
        public string ReceiptNumber { get; set; }
        public string ReferenceCode { get; set; }
        public string TrackingNumber { get; set; }
        public string Description { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ReceiptTime { get; set; }
        public int? IsActive { get; set; }
        public int? StatusID { get; set; }
        public HttpPostedFileBase ReceiptFile { get; set; }
    }
}