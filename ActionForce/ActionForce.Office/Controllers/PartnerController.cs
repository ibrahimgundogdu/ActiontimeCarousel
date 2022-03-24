using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class PartnerController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Partners = Db.Partner.Where(x=> x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }
    }
}