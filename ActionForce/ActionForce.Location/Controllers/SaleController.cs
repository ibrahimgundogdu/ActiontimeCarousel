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
        public ActionResult Index(string id)
        {
            SaleControlModel model = new SaleControlModel();

            DateTime selectedDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out selectedDate);
            }

            model.SelectedDate = selectedDate;
            model.SaleTotals = documentManager.GetDailySale(selectedDate);

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