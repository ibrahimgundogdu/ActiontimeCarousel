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
            model.Result = new Result() {IsSuccess = true, Message = $"{DateTime.Now.ToString()} deneme test hede höde deneme <b>ddd</b> deemljlj" };
            return View(model);
        }
    }
}