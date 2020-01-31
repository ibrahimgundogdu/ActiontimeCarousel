using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ComputeController : BaseController
    {
        // GET: Compute
        public ActionResult Index()
        {
            ComputeControlModel model = new ComputeControlModel();
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }
    }
}