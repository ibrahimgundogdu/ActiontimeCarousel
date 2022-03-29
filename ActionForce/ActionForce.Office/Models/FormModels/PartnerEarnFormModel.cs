using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnerEarnFormModel
    {
        public long DocumentEarnID { get; set; }
        public Guid UID { get; set; }

        public int? PartnerID { get; set; }
        public string ExpensePeriodCode { get; set; }
    }
}