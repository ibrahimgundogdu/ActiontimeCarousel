using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class MallController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            MallControlModel model = new MallControlModel();

            model.MallList = Db.VMall.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.StateList = model.MallList.Select(x => x.StateName).Distinct().OrderBy(x => x).ToList();
            model.CityList = model.MallList.Select(x => x.CityName).Distinct().OrderBy(x => x).ToList();
            model.CountryList = model.MallList.Select(x => x.CountryName).Distinct().OrderBy(x => x).ToList();

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult MallFilter(MallFilterModel getfilterMall)
        {
            return View();
        }
    }
}