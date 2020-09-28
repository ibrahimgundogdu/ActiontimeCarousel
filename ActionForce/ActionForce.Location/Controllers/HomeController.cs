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

            model.Summary = manager.GetLocationSummary(DateTime.Now.Date, model.Authentication.CurrentEmployee);

            return View(model);
        }
    }
}