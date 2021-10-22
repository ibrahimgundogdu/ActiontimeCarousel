using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class BankTransferStatusCount
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public int Count { get; set; }
        public double Amount { get; set; }
        public double Commission { get; set; }
        public string Currency { get; set; }

    }
}