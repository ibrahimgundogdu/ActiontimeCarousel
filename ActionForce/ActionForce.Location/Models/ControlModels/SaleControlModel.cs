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
        public Result Result { get; set; }
        public DateList DateKey { get; set; }

        public IEnumerable<SaleDayTotalModal> SaleTotals { get; set; }
        public IEnumerable<SaleDayTotalModal> RefundTotals { get; set; }

        public IEnumerable<LocationInfo> ActiveLocations { get; set; }
        public IEnumerable<LocationInfo> AppLocations { get; set; }
        public IEnumerable<OurCompany> OurCompanies { get; set; }
        public OurCompany CurrentOurCompany { get; set; }
        public IEnumerable<DailyEmployeeSalaryModel> SalaryList { get; set; }
        public IEnumerable<DailyCashExpense> ExpenseList { get; set; }
        


    }
}