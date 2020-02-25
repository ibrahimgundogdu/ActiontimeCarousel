using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class SaleControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public VPriceCategory PriceCategory { get; set; }
        public List<VPriceCategory> PriceCategoryList { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
    }
}