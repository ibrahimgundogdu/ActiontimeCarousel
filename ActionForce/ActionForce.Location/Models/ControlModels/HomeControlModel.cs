using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class HomeControlModel : LayoutControlModel
    {
        public IEnumerable<VPriceLastList> PriceList { get; set; }
        public VPriceLastList Price { get; set; }
        public VPrice VPrice { get; set; }

        public SummaryControlModel Summary { get; set; }

    }
}