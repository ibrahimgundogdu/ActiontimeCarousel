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
        public ActionResult Detail(Guid id)
        {
            MallControlModel model = new MallControlModel();

            if ( id== Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            model.MallModel = Db.VMall.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.MallUID == id);
            model.LocationList = Db.VLocation.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.MallID == null && x.IsActive == true).ToList();

            if (model.MallModel != null)
            {
                model.RelatedLocationList = Db.VLocation.Where(x => x.MallID == model.MallModel.ID && x.OurCompanyID== model.Authentication.ActionEmployee.OurCompanyID).ToList();
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RelatedMallContactDetail()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LinkToMall(MallControlModel model)
        {
            if (model != null)
            {
                Location willBeLinkedLocation = Db.Location.Where(x => x.LocationID == model.LocationModelID && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).FirstOrDefault();

                if (willBeLinkedLocation != null)
                {
                    willBeLinkedLocation.MallID = model.MallModel.ID;
                    Db.SaveChanges();

                    model.MallModel = Db.VMall.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID == model.MallModel.ID);

                    if (model.MallModel != null)
                    {
                        model.RelatedLocationList = Db.VLocation.Where(x => x.MallID == model.MallModel.ID && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                    }
                }
                return PartialView("_PartialRelatedLocationList", model);
            }

            model.Result.IsSuccess = false;
            model.Result.Message = "Lokasyon veya AVM Bilgisi Eksik";
            return PartialView("_PartialRelatedLocationList", model);
        }

        [AllowAnonymous]
        public ActionResult EditMall(Guid id)
        {
            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            MallControlModel model = new MallControlModel();
            model.MallModel = Db.VMall.FirstOrDefault(x => x.MallUID == id);
            model.OurCompanyList = Db.OurCompany.Where(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.MallSegmentList = Db.MallSegment.ToList();
            model.RelatedCountry = Db.Country.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.StateList = Db.State.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CityList = Db.VCity.Where(x => x.CountryID == model.RelatedCountry.ID).ToList();
            model.CountyList = Db.County.ToList();
            model.InvestorCompanyList = Db.Company.Where(x => x.CategoryID == 2 && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LeasingCompanyList = Db.Company.Where(x => x.CategoryID == 3 && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.PhoneCodes = Db.CountryPhoneCode.Where(x => x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            FormMall EditFormMall = new FormMall()
            {
                ID = model.MallModel.ID,
                FullName = model.MallModel.FullName.ToUpper(),
                MallSegmentID = model.MallModel.MallSegmentID,
                StructuralCondition = model.MallModel.StructuralCondition,
                Address = model.MallModel.Address,
                StateID = model.MallModel.StateID,
                CityID = model.MallModel.CityID,
                CountyID = model.MallModel.CountyID,
                PostCode = model.MallModel.PostCode,
                PhoneCountryCode = model.MallModel.PhoneCountryCode,
                PhoneNumber = model.MallModel.PhoneNumber.Replace("(", "").Replace(")", "").Replace(" ", "") ?? "",
                Latitude = model.MallModel.Latitude,
                Longitude= model.MallModel.Longitude,
                Map= model.MallModel.Map,
                InvestorCompanyID = model.MallModel.InvestorCompanyID,
                LeasingCompanyID = model.MallModel.LeasingCompanyID,
                Timezone= model.MallModel.TimeZone,
                IsActive = model.MallModel.IsActive == true ? "1" : ""
            };

            model.CheckMall = EditFormMall;

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult UpdateMall(FormMall toBeUpdatedModel)
        {
            MallControlModel model = new MallControlModel();

            if (toBeUpdatedModel != null)
            {
                Mall isMall = Db.Mall.FirstOrDefault(x => x.ID == toBeUpdatedModel.ID);
                toBeUpdatedModel.IsLeasingInHouse = toBeUpdatedModel.InvestorCompanyID == toBeUpdatedModel.LeasingCompanyID ? true : false;

                if (isMall != null)
                {
                    Mall selfMallModel = new Mall()
                    {
                        ID = isMall.ID,
                        OurCompanyID =isMall.OurCompanyID,
                        FullName=isMall.FullName,
                        MallSegmentID=isMall.MallSegmentID,
                        StructuralCondition=isMall.StructuralCondition,
                        Address=isMall.Address,
                        CountryID=isMall.CountryID,
                        StateID = isMall.StateID,
                        CityID=isMall.CityID,
                        CountyID=isMall.CountyID,
                        PostCode = isMall.PostCode,
                        PhoneCountryCode=isMall.PhoneCountryCode,
                        PhoneNumber=isMall.PhoneNumber,
                        Latitude=isMall.Latitude,
                        Longitude=isMall.Longitude,
                        Map=isMall.Map,
                        InvestorCompanyID=isMall.InvestorCompanyID,
                        LeasingCompanyID=isMall.LeasingCompanyID,
                        IsLeasingInHouse=isMall.IsLeasingInHouse,
                        ContactName=isMall.ContactName,
                        ContactTitle=isMall.ContactTitle,
                        ContactPhoneCode=isMall.ContactPhoneCode,
                        ContactPhone=isMall.ContactPhone,
                        ContactEmail=isMall.ContactEmail,
                        RecordDate=isMall.RecordDate,
                        RecordEmployeeID=isMall.RecordEmployeeID,
                        RecordIP=isMall.RecordIP,
                        UpdateDate=isMall.UpdateDate,
                        UpdateEmployeeID=isMall.UpdateEmployeeID,
                        UpdateIP=isMall.UpdateIP,
                        MallUID=isMall.MallUID,
                        TimeZone=isMall.TimeZone,
                        IsActive=isMall.IsActive
                    };

                    isMall.ID = toBeUpdatedModel.ID;
                    isMall.OurCompanyID = isMall.OurCompanyID;
                    isMall.FullName = toBeUpdatedModel.FullName;
                    isMall.MallSegmentID = toBeUpdatedModel.MallSegmentID;
                    isMall.StructuralCondition = toBeUpdatedModel.StructuralCondition;
                    isMall.Address = toBeUpdatedModel.Address;
                    isMall.CountryID = isMall.CountryID;
                    isMall.StateID = toBeUpdatedModel.StateID;
                    isMall.CityID = toBeUpdatedModel.CityID;
                    isMall.CountyID = toBeUpdatedModel.CountyID;
                    isMall.PostCode = toBeUpdatedModel.PostCode;
                    isMall.PhoneCountryCode = toBeUpdatedModel.PhoneCountryCode;
                    isMall.PhoneNumber = toBeUpdatedModel.PhoneNumber.Replace("(", "").Replace(")", "").Replace(" ", "") ?? "";
                    isMall.Latitude = toBeUpdatedModel.Latitude;
                    isMall.Longitude = toBeUpdatedModel.Longitude;
                    isMall.Map = toBeUpdatedModel.Map;
                    isMall.InvestorCompanyID = toBeUpdatedModel.InvestorCompanyID;
                    isMall.LeasingCompanyID = toBeUpdatedModel.LeasingCompanyID;
                    isMall.IsLeasingInHouse = toBeUpdatedModel.IsLeasingInHouse;
                    isMall.ContactName = isMall.ContactName;
                    isMall.ContactTitle = isMall.ContactTitle;
                    isMall.ContactPhoneCode = isMall.ContactPhoneCode;
                    isMall.ContactPhone = isMall.ContactPhone;
                    isMall.ContactEmail = isMall.ContactEmail;
                    isMall.RecordDate = isMall.RecordDate;
                    isMall.RecordEmployeeID = isMall.RecordEmployeeID;
                    isMall.RecordIP = isMall.RecordIP;
                    isMall.UpdateDate = DateTime.UtcNow.AddHours(toBeUpdatedModel.Timezone);
                    isMall.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    isMall.UpdateIP = OfficeHelper.GetIPAddress();
                    isMall.MallUID = isMall.MallUID;
                    isMall.TimeZone = toBeUpdatedModel.Timezone;
                    isMall.IsActive = !string.IsNullOrEmpty(toBeUpdatedModel.IsActive) && toBeUpdatedModel.IsActive == "1" ? true : false;

                    Db.SaveChanges();

                    model.Result = new Result
                    {
                        IsSuccess = true,
                        Message = $"{isMall.FullName} AVM'si güncellendi."
                    };

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Mall>(selfMallModel, isMall, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Mall", "Update", isMall.ID.ToString(), "Mall", "UpdateMall", isequal, model.Result.IsSuccess, model.Result.Message, string.Empty, DateTime.UtcNow.AddHours(toBeUpdatedModel.Timezone), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    return RedirectToAction("Detail", new { id = isMall.MallUID });
                }
                model.Result.IsSuccess = false;
                model.Result.Message = "Model Veritabanında Bulunamadı.";
                return RedirectToAction("Index");
            }

            model.Result.IsSuccess = false;
            model.Result.Message = "Güncellenecek Model Gönderilemedi.";
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult GetRefreshLocationList()
        {
            MallControlModel model = new MallControlModel();
            List<VLocation> refreshedList= Db.VLocation.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.MallID == null && x.IsActive == true).ToList();
            List<SelectListItem> SelectList = refreshedList.Select(x=> new SelectListItem() {
                Text = x.LocationFullName,
                Value = x.LocationID.ToString()
            }).ToList();

            return Json(SelectList, JsonRequestBehavior.AllowGet);
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