using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class SaleController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            SaleControlModel model = new SaleControlModel();
            return View(model);
        }
    }
}