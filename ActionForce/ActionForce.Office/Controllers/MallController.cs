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
            model.CityList = Db.VCity.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();

            if (TempData["MallFilter"] != null)
            {
                model.FilterModel = TempData["MallFilter"] as MallFilterModel;

                if (!String.IsNullOrEmpty(model.FilterModel.MallName))
                {
                    model.MallList = model.MallList.Where(x => x.FullName.Contains(model.FilterModel.MallName.ToUpper())).ToList();
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
            model.OurCompanyList = Db.OurCompany.Where(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.MallSegmentList = Db.MallSegment.ToList();
            model.RelatedCountry = Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.StateList = Db.State.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CityList = Db.VCity.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CountyList = Db.County.ToList();
            model.InvestorCompanyList = Db.Company.Where(x => x.CategoryID == 2).ToList();
            model.LeasingCompanyList = Db.Company.Where(x => x.CategoryID == 2).ToList();
            model.PhoneCodes= Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.LocationList = Db.VLocation.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

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
            mall.CountryID= Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ID;
            string OurCompanyName = Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).CountryName;
            mall.PhoneCountryCode = Db.CountryPhoneCode.FirstOrDefault(x => x.Country == OurCompanyName).Code;
            mall.IsLeasingInHouse = (mall.InvestorCompanyID == mall.LeasingCompanyID) ? true : false;

            if (mall!=null && isMall== null)
            {
                try
                {
                    mallUID = Guid.NewGuid();

                    Mall mallModel = new Mall()
                    {
                        OurCompanyID = mall.OurCompanyID,
                        FullName = mall.FullName,
                        MallSegmentID = mall.MallSegmentID,
                        StructuralCondition = mall.StructuralCondition,
                        Address = mall.Address,
                        CountryID = mall.CountryID,
                        StateID = mall.StateID,
                        CityID = mall.CityID,
                        CountyID = mall.CountyID,
                        PostCode = mall.PostCode,
                        PhoneCountryCode = mall.PhoneCountryCode,
                        PhoneNumber = mall.PhoneNumber,
                        Latitude = mall.Latitude,
                        Longitude = mall.Longitude,
                        Map = mall.Map,
                        InvestorCompanyID = mall.InvestorCompanyID,
                        LeasingCompanyID = mall.LeasingCompanyID,
                        IsLeasingInHouse = mall.IsLeasingInHouse,
                        ContactName=mall.ContactFullName,
                        ContactTitle=mall.Title,
                        ContactPhoneCode = mall.PhoneCode,
                        ContactPhone =mall.ContactPhoneNumber,
                        ContactEmail=mall.Email,
                        RecordDate = DateTime.UtcNow.AddHours(mall.Timezone),
                        RecordEmployeeID =model.Authentication.ActionEmployee.EmployeeID,
                        RecordIP=OfficeHelper.GetIPAddress(),
                        MallUID=mallUID,
                        TimeZone=mall.Timezone
                    };

                    Db.Mall.Add(mallModel);
                    Db.SaveChanges();

                    mall.MallID = Db.Mall.FirstOrDefault(x => x.FullName.Trim().ToUpper() == mall.FullName.Trim().ToUpper()).ID;

                    //Adding MallContact
                    MallContact mallContactModel = new MallContact()
                    {
                        MallID =mall.MallID,
                        Title=mall.Title,
                        FullName=mall.ContactFullName,
                        PhoneCode = mall.PhoneCode,
                        PhoneNumber =mall.ContactPhoneNumber,
                        Email=mall.Email,
                        IsMaster=true,
                        IsActive=mall.MallContactIsActive
                    };

                    Db.MallContact.Add(mallContactModel);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message= $"{mallModel.FullName} Alışveriş Merkezi Eklendi.";
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
            List<SelectListItem> countylist = new List<SelectListItem>();

            countylist = Db.County.Where(x => x.CityID == id).Select(x => new SelectListItem() { Value = x.ID.ToString(), Text = x.CountyName }).ToList();

            return Json(countylist, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetCityList(int? id)
        {
            List<SelectListItem> citylist = new List<SelectListItem>();

            citylist = Db.City.Where(x => x.StateID == id).Select(x => new SelectListItem() { Value = x.ID.ToString(), Text = x.CityName }).ToList();

            return Json(citylist, JsonRequestBehavior.AllowGet);
        }
    }
}