using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class SaleRowFormModel
    {
        public long OrderRowID { get; set; }
        public long OrderID { get; set; }
        public int PriceID { get; set; }
        public int ExtraUnit { get; set; }
        public float ExtraMultiply { get; set; }
        public DateTime ReceiptDate { get; set; }
        public TimeSpan ReceiptTime { get; set; }
        public string Description { get; set; }
    }
}