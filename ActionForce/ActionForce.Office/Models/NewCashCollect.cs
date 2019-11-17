using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class NewCashCollect
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
    }

    public class NewCashSale
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public int Quantity { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
    }
    public class NewCashExchange
    {
        public int ActinTypeID { get; set; }
        public int LocationID { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string Exchange { get; set; }

        public string DocumentDate { get; set; }
        public string Description { get; set; }
    }

    public class NewCashOpen
    {
        public int ActinTypeID { get; set; }
        public int LocationID { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
    }
}