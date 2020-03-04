using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
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

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            HttpCookie languageCookie = System.Web.HttpContext.Current.Request.Cookies["Language"];
            if (languageCookie != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCookie.Value);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCookie.Value);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");

            }

            base.Initialize(requestContext);
        }

    }
}