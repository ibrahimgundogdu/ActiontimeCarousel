using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class LocationBalance
    {
        public DateTime Date { get; set; } 
        public float CashTotal { get; set; }
        public float CreditTotal { get; set; }
        public float Balance { get; set; }
        public string  Currency { get; set; }
        public string  CurrencySign { get; set; }
    }
}