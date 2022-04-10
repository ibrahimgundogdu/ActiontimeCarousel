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
        public int Year { get; set; }
        public int Month { get; set; }
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

        public IEnumerable<LocationParam> LocationParameters { get; set; }
        public IEnumerable<LocationParamCalculate> LocationParamCalculate { get; set; }
        public IEnumerable<RevenueParameter> RevenueParameters { get; set; }
        public IEnumerable<LocationPeriods> PeriodParameters { get; set; }
        public IEnumerable<ActionType> ParameterTypes { get; set; }

        public FilterModel Filters { get; set; }
        public CounterModel Counters { get; set; }
        public List<ExpensePeriod> ExpensePeriods { get; set; }
        public List<VExpenseDocumentChart> ExpenseDocumentCharts { get; set; }
        public List<VExpenseSalePartnerless> ExpenseSalePartnerless { get; set; }
        public List<VExpenseSaleSystemless> ExpenseSaleSystemless { get; set; }
        public List<VPartnerActions> PartnerActions { get; set; }

        public List<Location> OfficeLocations { get; set; }
        public List<int> OfficeLocationIds { get; set; }

        public string SelectedPeriod { get; set; }

    }

    public class RevenueDetailModel
    {
        public VRevenue Revenue { get; set; }
        public IEnumerable<VRevenueLines> RevenueLines { get; set; }

    }

    public class LocationParameterDetailModel : LayoutControlModel
    {
        public Location Location { get; set; }
        public IEnumerable<LocationParam> LocationParameters { get; set; }
        public IEnumerable<LocationParamCalculate> LocationParamCalculate { get; set; }
    }

    public class RevenueParameterDetailModel : LayoutControlModel
    {
        public Location Location { get; set; }
        public IEnumerable<RevenueParameter> RevenueParameters { get; set; }
    }

    public class PeriodParameterDetailModel : LayoutControlModel
    {
        public Location Location { get; set; }
        public IEnumerable<LocationPeriods> PeriodParameters { get; set; }
    }

    public class RevenueComputeFilterModel : LayoutControlModel
    {
        public int? LocationID { get; set; }
        public int? WeekYear { get; set; }
        public int? WeekNumber { get; set; }
        public int? WeekNumberBegin { get; set; }
        public IEnumerable<VRevenue> Revenues { get; set; }
    }

}