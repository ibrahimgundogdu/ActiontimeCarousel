using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class LoginController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            LoginControlModel model = new LoginControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }

            return View(model);
        }
    }
}