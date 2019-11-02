using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class BaseController : Controller
    {
        public ActionTimeEntities Db { get; set; }

        public BaseController()
        {
            Db = new ActionTimeEntities();
        }
        protected override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && Db != null)
            {
                Db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}