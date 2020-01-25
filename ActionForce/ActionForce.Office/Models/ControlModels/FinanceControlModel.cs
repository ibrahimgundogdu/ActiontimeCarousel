using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FinanceControlModel : LayoutControlModel
    {
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Cash CurrentCash { get; set; }

    }
}