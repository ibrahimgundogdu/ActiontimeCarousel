using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class LocationController : BaseController
    {
        // GET: Location
        [AllowAnonymous]
        public ActionResult Index()
        {
            LocationControlModel model = new LocationControlModel();
            model.LocationList = Db.GetLocationAll(model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.StateList = model.LocationList.Select(x => x.State).Distinct().OrderBy(x=> x).ToList();
            model.TypeList = model.LocationList.Select(x => x.TypeName).Distinct().OrderBy(x=> x).ToList();
            return View(model);
        }
    }
}