using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class DefaultController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            DefaultControlModel model = new DefaultControlModel();

            return View(model);
        }
    }
}