using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SalesController : BaseController
    {
        // GET: Sales
        public ActionResult Index(string id)
        {
            SalesControlModel model = new SalesControlModel();
            
            return View(model);
        }
    }
}