using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class ActionsControlModel : LayoutControlModel
    {
        public List<VCashBankActions> ActionList { get; set; }
        public IEnumerable<TotalModel> HeaderTotals { get; set; }
        public IEnumerable<TotalModel> FooterTotals { get; set; }
        public DateTime DocumentDate { get; set; }
    }
}