using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class SaleController : BaseController
    {
        private readonly DocumentManager documentManager;
        public SaleController()
        {
            LayoutControlModel model = new LayoutControlModel();

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               LocationHelper.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentOurCompany.ID,
                   Name = model.Authentication.CurrentOurCompany.Name,
                   Currency = model.Authentication.CurrentOurCompany.Currency,
                   TimeZone = model.Authentication.CurrentOurCompany.TimeZone
               }
           );
        }


        [AllowAnonymous]
        public ActionResult Index(string id, int? OurCompnayID)
        {
            SaleControlModel model = new SaleControlModel();

            DateTime selectedDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out selectedDate);
            }

            

            model.DateKey = Db.DateList.FirstOrDefault(x => x.DateKey == selectedDate);
            model.OurCompanies = Db.OurCompany.ToList();
            model.SelectedDate = selectedDate;
            model.SaleTotals = documentManager.GetDailySale(selectedDate);
            model.RefundTotals = documentManager.GetDailySaleRefund(selectedDate);
            model.SalaryList = documentManager.GetDailyEmployeeSalary(selectedDate);
            model.ExpenseList = documentManager.GetDailyCashExpense(selectedDate);

            if (OurCompnayID != null && OurCompnayID > 0)
            {
                model.CurrentOurCompany = model.OurCompanies.FirstOrDefault(x => x.CompanyID == OurCompnayID);

                model.SaleTotals = documentManager.GetDailySale(selectedDate); //model.SaleTotals.Where(x=> x.OurCompanyID == OurCompnayID);
                model.RefundTotals = documentManager.GetDailySaleRefund(selectedDate);
                model.SalaryList = documentManager.GetDailyEmployeeSalary(selectedDate);
                model.ExpenseList = documentManager.GetDailyCashExpense(selectedDate);
            }

            model.ActiveLocations = Db.GetActiveLocations(selectedDate).Select(x => new LocationInfo()
            {
                ID = x.LocationID,
                Currency = x.Currency,
                FullName = x.LocationFullName,
                OurCompanyID = x.OurCompanyID,
                TimeZone = x.Timezone.Value,
                UID = x.LocationUID
            }).ToList();

            model.AppLocations = Db.GetAppLocations(selectedDate).Select(x => new LocationInfo()
            {
                ID = x.LocationID,
                Currency = x.Currency,
                FullName = x.LocationFullName,
                OurCompanyID = x.OurCompanyID,
                TimeZone = x.Timezone.Value,
                UID = x.LocationUID
            }).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            SaleControlModel model = new SaleControlModel();
            //model.PageTitle = $"{DateTime.Now.ToLongDateString()} &nbsp; &nbsp; <span class='font-weight-bold'> Sipariş Detayı </span>";

            return View(model);
        }
    }
}