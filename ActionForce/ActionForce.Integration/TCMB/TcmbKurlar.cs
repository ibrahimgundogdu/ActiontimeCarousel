using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Integration
{
    public class TcmbKurlar
    {
        public int totalCount { get; set; }
        public Item[] items { get; set; }
    }

    public class Item
    {
        public string Tarih { get; set; }
        public string TP_DK_USD_A { get; set; }
        public string TP_DK_USD_S { get; set; }
        public string TP_DK_EUR_A { get; set; }
        public string TP_DK_EUR_S { get; set; }
        public UNIXTIME UNIXTIME { get; set; }
        public object TP_DK_CHF_ATP_DK_GBP_A { get; set; }
    }

    public class UNIXTIME
    {
        public string numberLong { get; set; }
    }
}