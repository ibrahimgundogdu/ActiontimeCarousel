using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class FormCashExpense
    {
        public int TypeID { get; set; }
        public Guid? UID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ReceiptNumber { get; set; }
        public string Description { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime ReceiptTime { get; set; }
        public int? IsActive { get; set; }
        public HttpPostedFileBase ReceiptFile { get; set; }
    }
}