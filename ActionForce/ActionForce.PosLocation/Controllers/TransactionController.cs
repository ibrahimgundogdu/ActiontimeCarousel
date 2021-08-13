using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class TransactionController : BaseController
    {
        // GET: Transaction
        public ActionResult Index()
        {
            TransactionControlModel model = new TransactionControlModel();




            return View(model);
        }
    }
}