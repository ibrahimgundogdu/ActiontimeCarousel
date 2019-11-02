using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ActionController : BaseController
    {
        // GET: Actions
        public ActionResult Index(int? companyId, int? locationId)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == (locationId > 0 ? locationId : model.Authentication.ActionEmployee.OurCompanyID) && x.IsActive == true && x.LocationTypeID != 5 && x.LocationTypeID != 6);
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == (companyId > 0 ? companyId : model.Authentication.ActionEmployee.OurCompanyID));

            return View(model);
        }
    }
}