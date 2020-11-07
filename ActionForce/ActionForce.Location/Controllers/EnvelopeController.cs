using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class EnvelopeController : BaseController
    {

        private readonly DocumentManager documentManager;
        public EnvelopeController()
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


        // GET: Envelope
        [AllowAnonymous]
        public ActionResult Index()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();
            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);


            model.DocumentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == model.Location.ID && x.ProcessDate == model.DocumentDate.Date).ToList();
            model.EmployeeShifts = documentManager.GetEmployeeShifts(model.DocumentDate, model.Location.ID);
            model.TicketList = manager.GetLocationTicketsToday(model.DocumentDate);
            model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();
            model.LocationBalance = manager.GetLocationSaleBalanceToday(model.DocumentDate);
            model.Summary = manager.GetLocationSummary(model.DocumentDate, model.Authentication.CurrentEmployee);

            //List<int?> employeeids = model.EmployeeActions.Select(x => x.EmployeeID).ToList();



            return View(model);
        }

        [AllowAnonymous]
        public ActionResult CalculateSalary()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            DateTime? documentDate = model.DocumentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == model.Location.ID && x.Date == documentDate);

            if (dayresult != null)
            {
                var check = documentManager.CheckSalaryEarn(documentDate, model.Location.ID);
            }

            return RedirectToAction("Index");
        }

    }
}