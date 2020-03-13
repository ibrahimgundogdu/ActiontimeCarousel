using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

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
            model.InvestorCompanyList = Db.Company.Where(x => x.CategoryID == 2 && x.OurCompanyID==model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LeasingCompanyList = Db.Company.Where(x => x.CategoryID == 3 && x.OurCompanyID==model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.PhoneCodes= Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.LocationList = Db.VLocation.Where(x=>x.MallID==null && x.OurCompanyID==model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddMall(FormMall mall, HttpPostedFileBase ContractFile)
        {
            MallControlModel model = new MallControlModel();
            Guid mallUID = Guid.Empty;
            var isMall = Db.Mall.FirstOrDefault(x => x.FullName.Trim().ToUpper() == mall.FullName.Trim().ToUpper());
            mall.CountryID= Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ID;
            mall.IsLeasingInHouse = (mall.InvestorCompanyID == mall.LeasingCompanyID) ? true : false;
            mall.LocationDescription = mall.LocationDescription ?? Db.VLocation.FirstOrDefault(x => x.LocationID == mall.LocationID).LocationFullName;

            if (mall!=null && isMall== null)
            {
                try
                {
                    mallUID = Guid.NewGuid();

                    Mall mallModel = new Mall()
                    {
                        OurCompanyID = mall.OurCompanyID,
                        FullName = mall.FullName.ToUpper(),
                        MallSegmentID = mall.MallSegmentID,
                        StructuralCondition = mall.StructuralCondition,
                        Address = mall.Address,
                        CountryID = mall.CountryID,
                        StateID = mall.StateID,
                        CityID = mall.CityID,
                        CountyID = mall.CountyID,
                        PostCode = mall.PostCode,
                        PhoneCountryCode = mall.PhoneCountryCode,
                        PhoneNumber = mall.PhoneNumber.Replace("(", "").Replace(")", "").Replace(" ", "") ?? "",
                        Latitude = mall.Latitude,
                        Longitude = mall.Longitude,
                        Map = mall.Map,
                        InvestorCompanyID = mall.InvestorCompanyID,
                        LeasingCompanyID = mall.LeasingCompanyID,
                        IsLeasingInHouse = mall.IsLeasingInHouse,
                        ContactName = mall.ContactFullName,
                        ContactTitle = mall.Title,
                        ContactPhoneCode = mall.PhoneCode,
                        ContactPhone = mall.ContactPhoneNumber.Replace("(", "").Replace(")", "").Replace(" ", "") ?? "",
                        ContactEmail = mall.Email,
                        RecordDate = DateTime.UtcNow.AddHours(mall.Timezone),
                        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                        RecordIP = OfficeHelper.GetIPAddress(),
                        MallUID = mallUID,
                        TimeZone = mall.Timezone,
                        IsActive = !string.IsNullOrEmpty(mall.IsActive) && mall.IsActive == "1" ? true : false
                    };

                    Db.Mall.Add(mallModel);
                    Db.SaveChanges();

                    mall.MallID = Db.Mall.FirstOrDefault(x => x.FullName.Trim().ToUpper() == mall.FullName.Trim().ToUpper()).ID;

                    MallContact mallContactModel = new MallContact()
                    {
                        MallID =mall.MallID,
                        Title=mall.Title,
                        FullName=mall.ContactFullName,
                        PhoneCode = mall.PhoneCode,
                        PhoneNumber = mall.ContactPhoneNumber.Replace("(", "").Replace(")", "").Replace(" ", "") ?? "",
                        Email=mall.Email,
                        IsMaster=true,
                        IsActive = !string.IsNullOrEmpty(mall.MallContactIsActive) && mall.MallContactIsActive == "1" ? true : false
                    };

                    Db.MallContact.Add(mallContactModel);

                    if (ContractFile != null && ContractFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(ContractFile.FileName);
                        mall.ContractFile = filename;
                        string path = "/Document/Contract";

                        try
                        {
                            ContractFile.SaveAs(Path.Combine(Server.MapPath(path), filename));
                        }
                        catch (Exception)
                        {
                            model.Result.IsSuccess = false;
                            model.Result.Message = "Sözleşme Dosyası Yüklenirken Bir Hata Oluştu.";
                            return RedirectToAction("Index");
                        }
                    }
                    MallLocationContract mallLocationContractModel = new MallLocationContract();

                    mallLocationContractModel.MallID = mall.MallID;
                    mallLocationContractModel.LocationID = mall.LocationID;
                    mallLocationContractModel.LocationDescription = mall.LocationDescription;
                    mallLocationContractModel.RentAmount = Convert.ToDouble(mall.RentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    mallLocationContractModel.Currency = mall.Currency;
                    mallLocationContractModel.CommonExpenseAmount = !String.IsNullOrEmpty(mall.CommonExpenseAmount) ? Convert.ToDouble(mall.CommonExpenseAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : 0;
                    mallLocationContractModel.GuaranteeAmount = !String.IsNullOrEmpty(mall.GuaranteeAmount) ? Convert.ToDouble(mall.GuaranteeAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : 0;
                    mallLocationContractModel.StampDutyAmount = !String.IsNullOrEmpty(mall.StampDutyAmount) ? Convert.ToDouble(mall.StampDutyAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : 0;
                    mallLocationContractModel.ContractDateBegin = mall.ContractDateBegin;
                    mallLocationContractModel.ContractDateEnd = mall.ContractDateEnd;
                    mallLocationContractModel.ContractFile = mall.ContractFile;
                    mallLocationContractModel.IsActive = !string.IsNullOrEmpty(mall.MallContractIsActive) && mall.MallContractIsActive == "1" ? true : false;

                    Db.MallLocationContract.Add(mallLocationContractModel);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message= $"{mallModel.FullName} Alışveriş Merkezi Eklendi.";
                    TempData["result"] = model.Result;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Ekleme İşlemi Başarısız Oldu : "+ex.Message;
                    TempData["result"] = model.Result;
                    return RedirectToAction("Index");
                }
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"{mall.FullName} AVM zaten kayıtlı";
            TempData["result"] = model.Result;

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