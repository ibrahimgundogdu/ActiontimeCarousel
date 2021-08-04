using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class SaleItem
    {
        public int Quantity { get; set; }
        public int QuantityType { get; set; }
        public int TaxRate { get; set; }
        public string Title { get; set; }
        public long UnitAmount { get; set; }
    }
}