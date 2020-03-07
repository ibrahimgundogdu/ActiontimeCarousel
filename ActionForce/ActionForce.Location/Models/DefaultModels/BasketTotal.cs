using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class BasketTotal
    {
        public double Total { get; set; } = 0;
        public double Discount { get; set; } = 0;
        public double SubTotal { get; set; } = 0;
        public double TaxTotal { get; set; } = 0;
        public double GeneralTotal { get; set; } = 0;
        public string Currency { get; set; }
        public string Sign { get; set; }
    }
}