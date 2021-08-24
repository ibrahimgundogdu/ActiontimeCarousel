using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class FormExchangeSell
    {
        public Guid? UID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ToAmount { get; set; }
        public string ToCurrency { get; set; }
        public string SaleExchangeRate { get; set; }
        public string ReceiptNumber { get; set; }
        public string Description { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ReceiptTime { get; set; }
        public int? IsActive { get; set; }
        public HttpPostedFileBase ReceiptFile { get; set; }
    }
}