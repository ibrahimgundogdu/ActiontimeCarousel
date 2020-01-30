using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ActionForce.PosService.Controllers
{
    public class BaseController : ApiController
    {
        public ActionTimeEntities Db { get; private set; }

        public BaseController()
        {
            Db = new ActionTimeEntities();
        }

        public BaseController(ActionTimeEntities db)
        {
            Db = db;
        }

        protected override void Dispose(bool disposing)
        {
            if (Db != null && disposing)
            {
                Db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
