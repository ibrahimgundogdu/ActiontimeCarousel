using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ResultControlModel : LayoutControlModel
    {

        public VResult CurrentResult { get; set; }
        public Result Result { get; set; }
        public IEnumerable<VResult> ResultList { get; set; }
        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<Result> ResultMessage { get; set; }
    }
}