using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ActionForce.Entity;

namespace ActionForce.Office.Controllers
{
    public class LocationController : BaseController
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            LocationControlModel model = new LocationControlModel();

            model.LocationList = Db.VLocation.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.StateList = model.LocationList.Select(x => x.State).Distinct().OrderBy(x => x).ToList();
            model.LocationTypeList = Db.LocationType.ToList();

            #region Filter
            if (TempData["LocationFilter"] != null)
            {
                model.FilterModel = TempData["LocationFilter"] as LocationFilterModel;

                if (!String.IsNullOrEmpty(model.FilterModel.LocationName))
                {
                    model.LocationList = model.LocationList.Where(x => x.LocationNameSearch.Contains(model.FilterModel.LocationName.ToUpper())).OrderBy(x => x.SortBy).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.State))
                {
                    model.LocationList = model.LocationList.Where(x => x.State == model.FilterModel.State).OrderBy(x => x.SortBy).ToList();
                }
                if (!String.IsNullOrEmpty(model.FilterModel.TypeName))
                {
                    int locationTypeID = Convert.ToInt32(model.FilterModel.TypeName);
                    model.LocationList = model.LocationList.Where(x => x.LocationTypeID == locationTypeID).OrderBy(x => x.SortBy).ToList();
                }
            }
            #endregion

            #region Result
            //TODO: Lokasyon Update edildiği zaman buradaya gönderiliyor.
            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }
            #endregion

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult LocationFilter(LocationFilterModel getFilterModel)
        {
            TempData["LocationFilter"] = getFilterModel;

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult LocationSearch(LocationFilterModel getModel)
        {
            LocationControlModel model = new LocationControlModel();

            model.LocationList = Db.VLocation.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            #region Filter
            if (!String.IsNullOrEmpty(getModel.LocationName))
            {
                model.LocationList = model.LocationList.Where(x => x.LocationNameSearch.Contains(getModel.LocationName.ToUpper())).OrderBy(x => x.SortBy).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.State))
            {
                model.LocationList = model.LocationList.Where(x => x.State == getModel.State).OrderBy(x => x.SortBy).ToList();
            }
            if (!String.IsNullOrEmpty(getModel.TypeName))
            {
                model.LocationList = model.LocationList.Where(x => x.LocationTypeName == getModel.TypeName).OrderBy(x => x.SortBy).ToList();
            }

            bool? isActive = getModel.IsActive == 0 ? false : getModel.IsActive == 1 ? true : (bool?)null;

            if (isActive != null)
            {
                model.LocationList = model.LocationList.Where(x => x.IsActive == isActive.Value).ToList();
            }

            #endregion

            return PartialView("_PartialLocationList", model);
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid id)
        {
            LocationControlModel model = new LocationControlModel();

            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationUID == id);

            if (model.LocationModel != null)
            {
                model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Location" && x.ProcessID == model.LocationModel.LocationID.ToString()).ToList();
                model.EmployeeLocationList = Db.GetLocationEmployees(model.LocationModel.LocationID).Select(x => new LocationEmployeeModel()
                {
                    EmployeeID = x.EmployeeID,
                    EmployeeUID = x.EmployeeUID ?? Guid.Empty,
                    FullName = x.FullName,
                    Active = x.Active ?? false,
                    PositionName = x.PositionName
                }).ToList();
                #region Schedule
                model.ScheduleStart = model.LocationModel.ScheduleStart?.ToShortTimeString();
                model.ScheduleFinish = model.LocationModel.ScheduleEnd?.ToShortTimeString();
                model.ScheduleTime = model.LocationModel.ScheduleDuration?.ToString("hh\\:mm");
                #endregion
                #region Shift
                model.ShiftStart = model.LocationModel.ShiftStart?.ToString("hh\\:mm");
                model.ShiftFinish = model.LocationModel.ShiftFinish?.ToString("hh\\:mm");
                model.ShiftTime = model.LocationModel.Duration;
                #endregion
                #region Status
                model.StatusName = (model.LocationModel.Status != null ? (model.LocationModel.Status == 0 ? "Beklemede" : (model.LocationModel.Status == 1 ? "Açık" : (model.LocationModel.Status == 2 ? "Kapalı" : ""))) : "");
                model.StatusClass = (model.LocationModel.Status != null ? (model.LocationModel.Status == 0 ? "warning" : (model.LocationModel.Status == 1 ? "success" : (model.LocationModel.Status == 2 ? "danger" : "danger"))) : "danger");
                model.StatusIcon = (model.LocationModel.Status != null ? (model.LocationModel.Status == 0 ? "clock" : (model.LocationModel.Status == 1 ? "sun" : (model.LocationModel.Status == 2 ? "moon" : "moon"))) : "moon");
                #endregion
                #region ScheduleLocation
                model.WeekCode = model.LocationModel.WeekKey.Trim();
                model.LocationScheduleList = Db.VLocationSchedule.Where(x => x.WeekCode.Trim() == model.WeekCode && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == model.LocationModel.LocationID).ToList();
                #endregion
                #region ShiftLocation
                model.WeekList = Db.DateList.Where(x => x.WeekKey == model.LocationModel.WeekKey).ToList();
                model.LocationShiftList = Db.VLocationShift.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == model.LocationModel.LocationID && x.WeekKey == model.LocationModel.WeekKey).ToList();
                #endregion
            }
            else
            {
                return RedirectToAction("Index", "Location");
            }

            return View(model);
        }

        [AllowAnonymous] /* TODO: Sonrasında kaldırılacak yetkiye bağlanacak. [Permision] Tablosu ve [RoleGroupPermissions]*/
        public ActionResult Edit(Guid id)
        {
            LocationControlModel model = new LocationControlModel();

            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationUID == id);
            model.LocationTypeList = Db.LocationType.ToList();

            if (model.LocationModel != null)
            {
                model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Location" && x.ProcessID == model.LocationModel.LocationID.ToString()).ToList();
                model.OurCompanyList = Db.OurCompany.ToList();
                model.PriceCategoryList = Db.PriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.TicketTypeID == model.LocationModel.TicketTypeID).ToList();
                model.MallList = Db.Mall.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 2).ToList(); // TODO: AccountTypeID == 2 olmasının sebebi POS işlemlerinden dolayı
                #region Schedule
                model.ScheduleStart = model.LocationModel.ScheduleStart?.ToShortTimeString();
                model.ScheduleFinish = model.LocationModel.ScheduleEnd?.ToShortTimeString();
                model.ScheduleTime = model.LocationModel.ScheduleDuration?.ToString("hh\\:mm");
                #endregion
                #region Shift
                model.ShiftStart = model.LocationModel.ShiftStart?.ToString("hh\\:mm");
                model.ShiftFinish = model.LocationModel.ShiftFinish?.ToString("hh\\:mm");
                model.ShiftTime = model.LocationModel.Duration;
                #endregion
                #region Status
                model.StatusName = (model.LocationModel.Status != null ? (model.LocationModel.Status == 0 ? "Beklemede" : (model.LocationModel.Status == 1 ? "Açık" : (model.LocationModel.Status == 2 ? "Kapalı" : ""))) : "");
                model.StatusClass = (model.LocationModel.Status != null ? (model.LocationModel.Status == 0 ? "warning" : (model.LocationModel.Status == 1 ? "success" : (model.LocationModel.Status == 2 ? "danger" : "danger"))) : "danger");
                model.StatusIcon = (model.LocationModel.Status != null ? (model.LocationModel.Status == 0 ? "clock" : (model.LocationModel.Status == 1 ? "sun" : (model.LocationModel.Status == 2 ? "moon" : "moon"))) : "moon");
                #endregion
            }
            else
            {
                return RedirectToAction("Index", "Location");
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetTimezoneList(int ourCompanyId)
        {
            var getOurCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == ourCompanyId);

            List<SelectListItem> list = new List<SelectListItem>();
            List<SelectListItem> priceList = new List<SelectListItem>();
            List<SelectListItem> mallList = new List<SelectListItem>();

            int start, finish = 0;

            start = getOurCompany.TimeZone.Value > getOurCompany.TimeZoneTo.Value ? getOurCompany.TimeZone.Value : getOurCompany.TimeZoneTo.Value;
            finish = getOurCompany.TimeZone.Value < getOurCompany.TimeZoneTo.Value ? getOurCompany.TimeZone.Value : getOurCompany.TimeZoneTo.Value;

            for (int i = start; i >= finish; i--)
            {
                list.Add(new SelectListItem()
                {
                    Value = i.ToString(),
                    Text = i > 0 ? "+" + i : i.ToString()
                });
            }

            priceList = Db.PriceCategory.Where(x => x.OurCompanyID == ourCompanyId).Select(y => new SelectListItem() { Value = y.ID.ToString(), Text = y.CategoryName }).ToList();
            mallList = Db.Mall.Where(x => x.OurCompanyID == ourCompanyId).Select(m => new SelectListItem() { Value = m.ID.ToString(), Text = m.FullName }).ToList();

            OurCompanyModel model = new OurCompanyModel()
            {
                Currency = getOurCompany.Currency,
                SelectList = list,
                PriceCategoryList = priceList,
                MallList = mallList
            };

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public ActionResult EditLocation(CULocation location)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();

            DateTime daterecord = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
            var isLocation = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID && x.LocationUID == location.LocationUID);

            if (location != null && isLocation != null)
            {
                try
                {
                    #region SelfModel
                    Location self = new Location()
                    {
                        Currency = isLocation.Currency,
                        Description = isLocation.Description,
                        Distance = isLocation.Distance,
                        EnforcedWarning = isLocation.EnforcedWarning,
                        ImageFile = isLocation.ImageFile,
                        IP = isLocation.IP,
                        IsActive = isLocation.IsActive,
                        IsHaveOperator = isLocation.IsHaveOperator,
                        Latitude = isLocation.Latitude,
                        LocalDate = isLocation.LocalDate,
                        LocalDateTime = isLocation.LocalDateTime,
                        LocationCode = isLocation.LocationCode,
                        LocationFullName = isLocation.LocationFullName,
                        LocationID = isLocation.LocationID,
                        LocationName = isLocation.LocationName,
                        LocationNameSearch = isLocation.LocationNameSearch,
                        LocationTypeID = isLocation.LocationTypeID,
                        LocationUID = isLocation.LocationUID,
                        Longitude = isLocation.Longitude,
                        MallID = isLocation.MallID,
                        MapURL = isLocation.MapURL,
                        OurCompanyID = isLocation.OurCompanyID,
                        POSAccountID = isLocation.POSAccountID,
                        PriceCatID = isLocation.PriceCatID,
                        RecordDate = isLocation.RecordDate,
                        RecordEmployeeID = isLocation.RecordEmployeeID,
                        RecordIP = isLocation.RecordIP,
                        SortBy = isLocation.SortBy,
                        State = isLocation.State,
                        Timezone = isLocation.Timezone,
                        Weight = isLocation.Weight
                    };
                    #endregion
                    #region UpdateModel
                    isLocation.Currency = location.Currency;
                    isLocation.IP = location.IP;
                    isLocation.IsActive = !String.IsNullOrEmpty(location.IsActive) && location.IsActive == "1" ? true : false; ;
                    isLocation.IsHaveOperator = !String.IsNullOrEmpty(location.IsHaveOperator) && location.IsHaveOperator == "1" ? true : false; ;
                    isLocation.Latitude = location.Latitude;
                    isLocation.LocationCode = location.LocationCode;
                    isLocation.LocationID = location.LocationID;
                    isLocation.LocationName = location.LocationName;
                    isLocation.LocationNameSearch = location.LocationNameSearch;
                    isLocation.LocationTypeID = location.LocationTypeID;
                    isLocation.Description = Db.LocationType.FirstOrDefault(x => x.ID == location.LocationTypeID).TypeName;
                    isLocation.LocationUID = location.LocationUID;
                    isLocation.Longitude = location.Longitude;
                    isLocation.MallID = location.MallID;
                    isLocation.MapURL = location.MapURL;
                    isLocation.OurCompanyID = location.OurCompany;
                    isLocation.POSAccountID = location.POSAccountID;
                    isLocation.PriceCatID = location.PriceCatID;
                    isLocation.UpdateDate = DateTime.UtcNow.AddHours(location?.Timezone ?? 0);
                    isLocation.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    isLocation.UpdateIP = OfficeHelper.GetIPAddress();
                    isLocation.SortBy = location.SortBy;
                    isLocation.State = location.State;
                    isLocation.Timezone = location.Timezone;
                    #endregion
                    #region PriceCategoryCheck
                    if (self.PriceCatID != isLocation.PriceCatID)
                    {
                        LocationPriceCategory priceCat = new LocationPriceCategory()
                        {
                            LocationID = isLocation?.LocationID,
                            PriceCategoryID = isLocation?.PriceCatID,
                            StartDate = DateTime.UtcNow.AddHours(isLocation?.Timezone ?? 0),
                            RecordDate = DateTime.UtcNow.AddHours(isLocation?.Timezone ?? 0),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = OfficeHelper.GetIPAddress()
                        };

                        Db.LocationPriceCategory.Add(priceCat);
                    } 
                    #endregion

                    Db.SaveChanges();

                    #region ResultMessage
                    model.Result.IsSuccess = true;
                    model.Result.Message = $"{isLocation.LocationName} Lokasyonu güncellendi.";
                    #endregion
                    #region AddLog
                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<Location>(self, isLocation, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "Location", "Update", isLocation.LocationID.ToString(), "Location", "EditLocation", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(isLocation?.Timezone ?? 0), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    #endregion
                }
                catch (Exception ex)
                {
                    model.Result.Message = ex.Message;
                    model.Result.IsSuccess = false;
                }

                TempData["result"] = model.Result;
            }

            return RedirectToAction("Index", "Location");
        }

        [AllowAnonymous]
        public ActionResult Add()
        {
            LocationControlModel model = new LocationControlModel();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult LocationCheck()
        {

            LocationControlModel model = new LocationControlModel();

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }

    #region OurCompanyModel
    //TODO: SelectListItem MVC'den türediği için başka bir sınıfın içerisine alamadığımızdan dolayı OurCompanyModel sınıfı burada bulunmaktadır.
    public class OurCompanyModel
    {
        public List<SelectListItem> SelectList { get; set; }
        public List<SelectListItem> PriceCategoryList { get; set; }
        public List<SelectListItem> MallList { get; set; }
        public string Currency { get; set; }
    }
    #endregion
}