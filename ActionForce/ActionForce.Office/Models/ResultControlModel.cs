using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ResultControlModel : LayoutControlModel
    {

        public VDayResult CurrentDayResult { get; set; }
        public DayResult DayResult { get; set; }
        public IEnumerable<VDayResult> DayResultList { get; set; }
        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<VLocationSchedule> LocationScheduleList { get; set; }
        public IEnumerable<VLocationShift> LocationShiftList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public Location CurrentLocation { get; set; }
        public DateList CurrentDate { get; set; }
        public Result<DayResult> Result { get; set; }

        
        public string TodayDateCode { get; set; }
        public string CurrentDateCode { get; set; }
        public string NextDateCode { get; set; }
        public string PrevDateCode { get; set; }
    }
}