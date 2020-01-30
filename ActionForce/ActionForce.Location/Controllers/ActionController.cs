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
            model.PageTitle = $"{DateTime.Now.ToLongDateString()} &nbsp; &nbsp; <span class='font-weight-bold'> Action </span>";

            return View(model);
        }
    }
}