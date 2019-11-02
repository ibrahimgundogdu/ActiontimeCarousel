using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class FinanceController : BaseController
    {
        // GET: Finance
        public ActionResult CashCollection(int? companyId, int? locationId, int? id)
        {
            FinanceControlModel model = new FinanceControlModel();


            return View(model);
        }
    }
}