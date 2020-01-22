using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class RevenueControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public int WeekYear { get; set; }
        public int WeekNumber { get; set; }
        public IEnumerable<Location> Locations { get; set; }
        public VRevenue Revenue { get; set; }
        public IEnumerable<VRevenue> Revenues { get; set; }
        public IEnumerable<VRevenueLines> RevenueLines { get; set; }


        public IEnumerable<DateList> WeekList { get; set; }
        public DateList FirstWeekDay { get; set; }
        public DateList LastWeekDay { get; set; }
        public DateList SelectedDate { get; set; }
        public DateList CurrentDate { get; set; }

        public DateList NextWeek { get; set; }
        public DateList PrevWeek { get; set; }
    }
}