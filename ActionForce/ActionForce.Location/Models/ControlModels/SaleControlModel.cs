using ActionForce.Entity;
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
        public DateList DateKey { get; set; }
        public IEnumerable<VPriceLastList> PriceList { get; set; }

        public IEnumerable<SaleDayTotalModal> SaleTotals { get; set; }
        public IEnumerable<SaleDayLocationModal> LocationSaleTotals { get; set; }
        public IEnumerable<SaleDayTotalModal> RefundTotals { get; set; }
        public IEnumerable<LocationInfo> ActiveLocations { get; set; }
        public IEnumerable<LocationInfo> AppLocations { get; set; }
        public IEnumerable<OurCompany> OurCompanies { get; set; }
        public OurCompany CurrentOurCompany { get; set; }
        public LocationInfo CurrentLocation { get; set; }
        public IEnumerable<DailyEmployeeSalaryModel> SalaryList { get; set; }
        public IEnumerable<DailyCashExpense> ExpenseList { get; set; }
        public IEnumerable<LocationTicketSaleInfo> TicketList { get; set; }
        public LocationBalance LocationBalance { get; set; }

        public List<VDocumentsAllSummaryRevenue> Revenue { get; set; }



    }
}