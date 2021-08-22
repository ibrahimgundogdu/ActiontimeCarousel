using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class SummaryControlModel
    {
        public IEnumerable<VCashActions> CashActions { get; set; }
        public Cash CurrentCash { get; set; }
        public IEnumerable<TotalModel> DevirTotal { get; set; }
        public IEnumerable<VBankActions> BankActions { get; set; }
        public DayResult DayResult { get; set; }
        public IEnumerable<ResultState> ResultStates { get; set; }
        public Result<DayResult> Result { get; set; }

        public VDayResult CurrentDayResult { get; set; }
    }
}