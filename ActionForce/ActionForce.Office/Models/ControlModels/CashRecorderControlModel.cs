using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CashRecorderControlModel : LayoutControlModel
    {
        public IEnumerable<VDocumentCashRecorderSlip> CashRecorder { get; set; }
        public IEnumerable<VCashRecorderMuhasebe> CashRecorderMuhasebeList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<MonthList> MonthList { get; set; }
        public IEnumerable<LocationList> LocationListItems { get; set; }
        public IEnumerable<MonthList> MonthListItems { get; set; }
        public DateTime SelectedDate { get; set; }

        public VDocumentCashRecorderSlip Detail { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<CashActions> Result { get; set; }
        public IEnumerable<ApplicationLog> History { get; set; }
    }
}