using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class UfePlusCardController : BaseController
    {
        // GET: Ufecard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Payment(long? id)
        {
            UfePlusCardControlModel model = new UfePlusCardControlModel();
            model.Authentication = this.AuthenticationData;

            if (id == null)
            {
                return RedirectToAction("Index", "Sales");
            }

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }


            model.SaleSummary = Db.VTicketSaleSummary.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ID == id);


            return View(model);
        }
    }
}