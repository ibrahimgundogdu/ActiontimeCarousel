using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class Invoice
    {
        public int InvoiceType { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string TCKN_VKN { get; set; }
        //public string CustomerName { get; set; }
    }
}