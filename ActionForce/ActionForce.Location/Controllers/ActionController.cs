using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class ActionController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            ActionControlModel model = new ActionControlModel();

            return View(model);
        }
    }
}