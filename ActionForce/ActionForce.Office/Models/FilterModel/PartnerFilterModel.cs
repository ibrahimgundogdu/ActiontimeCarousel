using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnerFilterModel
    {
        public int? PartnerID { get; set; }
        public int? LocationID { get; set; }
        public string ExpensePeriodCode { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }


        public string PeriodCodeBegin { get; set; }
        public string PeriodCodeEnd { get; set; }


    }
}