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
            model.PageTitle = $"{DateTime.Now.ToLongDateString()} &nbsp; &nbsp; <span class='font-weight-bold'> Sales </span>";

            return View(model);
        }
    }
}