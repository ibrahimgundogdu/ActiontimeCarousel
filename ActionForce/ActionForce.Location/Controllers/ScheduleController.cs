using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class ScheduleController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            ScheduleControlModel model = new ScheduleControlModel();
            return View(model);
        }
    }
}