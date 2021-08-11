using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class BaseController : Controller
    {
        public ActionTimeEntities Db { get; set; } = new ActionTimeEntities();


        public BaseController()
        {
            Db = new ActionTimeEntities();
        }
        protected override void OnActionExecuting(ActionExecutingContext context)
        {

            // 01. lokasyon Kontrolü
            var controller = context.RouteData.Values["controller"].ToString();
            var action = context.RouteData.Values["action"].ToString();


            if (context.RouteData.Values["controller"].ToString() != "Setup")
            {
                HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

                if (locationCookie == null)
                {
                    context.Result = new RedirectResult("/Setup/Index");
                    return;
                }
            }

            







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