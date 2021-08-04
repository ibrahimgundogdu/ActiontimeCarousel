using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class Discount
    {
        public int IndexOffItem { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
        public int Value { get; set; }
        public int Orjin { get; set; }

    }
}