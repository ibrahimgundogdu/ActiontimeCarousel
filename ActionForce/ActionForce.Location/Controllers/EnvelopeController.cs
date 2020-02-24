using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class EnvelopeController : BaseController
    {
        // GET: Envelope
        [AllowAnonymous]
        public ActionResult Index()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            return View(model);
        }
    }
}