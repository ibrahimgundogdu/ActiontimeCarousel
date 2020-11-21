using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{

    public class SaleControlModel : LayoutControlModel
    {
        public DateTime SelectedDate { get; set; }
        public Result Result { get; set; }

        public IEnumerable<SaleDayTotalModal> SaleTotals { get; set; }

    }
}