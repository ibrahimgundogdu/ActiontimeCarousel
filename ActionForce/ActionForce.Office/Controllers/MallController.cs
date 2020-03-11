using ActionForce.Entity;
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
            model.RelatedCountry = Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.StateList = Db.State.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CityList = Db.City.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();

            if (TempData["MallFilter"] != null)
            {
                model.FilterModel = TempData["MallFilter"] as MallFilterModel;

                if (!String.IsNullOrEmpty(model.FilterModel.MallName))
                {
                    model.MallList = model.MallList.Where(x => x.FullName.Contains(model.FilterModel.MallName.ToUpper())).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.CountryName))
                {
                    model.MallList = model.MallList.Where(x => x.CountryName == model.FilterModel.CountryName).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.StateName))
                {
                    model.MallList = model.MallList.Where(x => x.StateName == model.FilterModel.StateName).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.CityName))
                {
                    model.MallList = model.MallList.Where(x => x.CityName == model.FilterModel.CityName).ToList();
                }
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult MallFilter(MallFilterModel getfilterModel)
        {
            TempData["MallFilter"] = getfilterModel;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult MallSearch(MallFilterModel getModel)
        {
            MallControlModel model = new MallControlModel();

            model.MallList = Db.VMall.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (!String.IsNullOrEmpty(getModel.MallName))
            {
                model.MallList = model.MallList.Where(x => x.FullName.Contains(getModel.MallName)).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.CountryName))
            {
                model.MallList = model.MallList.Where(x => x.CountryName == getModel.CountryName).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.StateName))
            {
                model.MallList = model.MallList.Where(x => x.StateName == getModel.StateName).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.CityName))
            {
                model.MallList = model.MallList.Where(x => x.CityName == getModel.CityName).ToList();
            }

            bool? isActive = getModel.IsActive == 0 ? false : getModel.IsActive == 1 ? true : (bool?)null;

            if (isActive != null)
            {
                model.MallList = model.MallList.Where(x => x.IsActive == isActive.Value).ToList();
            }

            return PartialView("_PartialMallList", model);
        }

        [AllowAnonymous]
        public ActionResult Add()
        {
            MallControlModel model = new MallControlModel();
            model.ExistingCities = Db.VCity.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.OurCompanyList = Db.OurCompany.Where(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.MallSegmentList = Db.MallSegment.ToList();
            model.RelatedCountry = Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.StateList = Db.State.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CityList = Db.City.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CountyList = Db.County.ToList();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult AddMall(FormMall mall)
        {
            MallControlModel model = new MallControlModel();
            model.Result = new Result();
            Guid mallUID = Guid.Empty;
            var isMall = Db.Mall.FirstOrDefault(x => x.FullName.Trim().ToUpper() == mall.FullName.Trim().ToUpper());

            if (mall != null)
            {
                try
                {
                    mallUID = new Guid();

                    Mall mallModel = new Mall()
                    {
                        OurCompanyID = mall.OurCompanyID,

                    };
                }
                catch (Exception ex)
                {

                    throw;
                }
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetCountyList(int? id)
        {
            MallControlModel model = new MallControlModel();
            model.Result = new Result();

            List<SelectListItem> countylist = new List<SelectListItem>();

            countylist = Db.County.Where(x => x.CityID == id).Select(x => new SelectListItem() { Value = x.ID.ToString(), Text = x.CountyName }).ToList();

            return Json(countylist, JsonRequestBehavior.AllowGet);
        }
    }
}