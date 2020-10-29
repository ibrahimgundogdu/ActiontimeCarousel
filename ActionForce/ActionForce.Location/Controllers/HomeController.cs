using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        [AllowAnonymous]
        public ActionResult Index()
        {
            HomeControlModel model = new HomeControlModel();

            model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();

            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);

            //model.Summary = manager.GetLocationSummary(DateTime.Now.Date, model.Authentication.CurrentEmployee);
            model.LocationBalance = manager.GetLocationSaleBalanceToday();
            model.TicketList = manager.GetLocationTicketsToday();

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult GetTicketModal(int PriceID, int PaymethodID)
        {
            StandartTicket model = new StandartTicket();

            model.Price = Db.VPrice.FirstOrDefault(x => x.ID == PriceID);
            model.PayMethodID = PaymethodID;

            if (model.Price.TicketTypeID == 2)
            {
                model.AnimalCostumes = Db.GetLocationAnimalCostums(model.Location.ID).Select(x => new AnimalCostume()
                {
                    CostumeID = x.ID,
                    CostumeName = x.CostumeName
                }).ToList();
            }

            if (model.Price.TicketTypeID == 7)
            {
                model.MallMotoColor = Db.GetLocationMallMotoColors(model.Location.ID).Select(x => new MallMotoColor()
                {
                    ColorID = x.ID,
                    ColorName = x.ColorName
                }).ToList();
            }

            return PartialView("_PartialTicket", model);
        }

        [AllowAnonymous]
        public string SetTicketSale(int PriceID, int PaymethodID, int? ColorID, int? CostumeID)
        {
            string message = string.Empty;

            StandartTicket model = new StandartTicket();
            var date = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            model.Price = Db.VPrice.FirstOrDefault(x => x.ID == PriceID);
            model.PayMethodID = PaymethodID;
            var OrderNumber = string.Format("S{0}{1}{2}{3}", date.Year.ToString().Substring(2, 2), date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString(), date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString(), date.Ticks.ToString());
            string ip = LocationHelper.GetIPAddress();

            var rowID = Db.AddLocationTicketSale(model.Location.ID, date, PriceID, PaymethodID, 2, model.Authentication.CurrentEmployee.EmployeeID, ColorID > 0 ? ColorID : null, CostumeID > 0 ? CostumeID : null, OrderNumber, ip).FirstOrDefault();

            if (rowID > 0)
            {
                message = "Satış Eklendi";
                LocationHelper.AddApplicationLog("Location", "TicketSaleRows", "Insert", rowID.ToString(), "Home", "SetTicketSale", null, true, message, string.Empty, date, $"{model.Authentication.CurrentEmployee.EmployeeID} - {model.Authentication.CurrentEmployee.FullName}", LocationHelper.GetIPAddress(), string.Empty, null);
            }
            else
            {
                message = "Satış Eklenemedi";
            }

            return message;
        }
    }
}