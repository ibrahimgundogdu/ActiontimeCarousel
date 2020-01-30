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
            model.PageTitle = $"{DateTime.Now.ToLongDateString()} &nbsp; &nbsp; <span class='font-weight-bold'> Schedule & Shift </span>";

            return View(model);
        }
    }
}