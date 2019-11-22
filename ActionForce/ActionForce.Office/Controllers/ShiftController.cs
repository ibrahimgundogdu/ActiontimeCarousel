using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ShiftController : BaseController
    {
        public ActionResult Index()
        {
            ShiftControlModel model = new ShiftControlModel();

            return View(model);
        }
        public ActionResult LocationShift()
        {
            ShiftControlModel model = new ShiftControlModel();

            return View(model);
        }
        public ActionResult EmployeeShift()
        {
            ShiftControlModel model = new ShiftControlModel();

            return View(model);
        }
    }
}