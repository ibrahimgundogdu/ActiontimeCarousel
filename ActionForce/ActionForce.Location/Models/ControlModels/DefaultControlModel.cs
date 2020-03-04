using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class DefaultControlModel : LayoutControlModel
    {
        public IEnumerable<VPriceLastList> PriceList { get; set; }
        //public VPriceLastList Price { get; set; }
    }
}