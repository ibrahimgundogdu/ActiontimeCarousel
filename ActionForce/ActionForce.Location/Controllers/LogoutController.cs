using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ActionForce.Location.Controllers
{
    public class LogoutController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }
    }
}