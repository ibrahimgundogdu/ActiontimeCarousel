using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class SubPayment
    {
        public int Type { get; set; }
        public int Amount { get; set; }
        public string Name { get; set; }
    }
}