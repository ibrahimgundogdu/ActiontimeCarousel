using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class MallController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            MallControlModel model = new MallControlModel();

            return View();
        }
    }
}