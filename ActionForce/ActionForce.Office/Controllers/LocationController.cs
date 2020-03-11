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
        #region Location
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
                model.LocationPriceLastList = Db.GetLocationPrice(model.LocationModel.LocationID,model.LocationModel.LocalDateTime).ToList();
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
                model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.TicketTypeID == model.LocationModel.TicketTypeID).ToList();
                model.MallList = Db.Mall.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 2).ToList(); // TODO: AccountTypeID == 2 olmasının sebebi POS işlemlerinden dolayı
                model.CityList = Db.VCity.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.LocationPosTerminalList = Db.VLocationPosTerminal.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == model.LocationModel.LocationID).ToList();
                model.TaxList = Db.Tax.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
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

            if (TempData["checkLocation"] != null)
            {
                model.CheckLocation = TempData["checkLocation"] as FormLocation;

                var ticketTypeId = Db.LocationType.FirstOrDefault(x => x.ID == model.CheckLocation.LocationTypeID).TicketTypeID;
                model.PriceCategoryList = model.PriceCategoryList.Where(x => x.TicketTypeID == ticketTypeId).ToList();
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetTimezoneList(int? ourCompanyId, int? locationTypeId)
        {
            LocationControlModel model = new LocationControlModel();
            List<SelectListItem> list = new List<SelectListItem>();
            List<SelectListItem> priceList = new List<SelectListItem>();
            List<SelectListItem> mallList = new List<SelectListItem>();
            List<SelectListItem> posList = new List<SelectListItem>();
            List<SelectListItem> cityList = new List<SelectListItem>();
            int start, finish = 0;

            ourCompanyId = (ourCompanyId ?? model.Authentication.ActionEmployee.OurCompanyID);

            var getOurCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == ourCompanyId);

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

            var getPriceList = Db.VPriceCategory.Where(x => x.OurCompanyID == ourCompanyId).ToList();

            if (locationTypeId != null && locationTypeId > 0)
            {
                var ticketTypeId = Db.LocationType.FirstOrDefault(x => x.ID == locationTypeId).TicketTypeID;
                getPriceList = getPriceList.Where(x => x.TicketTypeID == ticketTypeId).ToList();
            }

            priceList = getPriceList.Select(y => new SelectListItem() { Value = y.ID.ToString(), Text = y.TicketTypeName + " " + y.CategoryName }).ToList();
            mallList = Db.Mall.Where(x => x.OurCompanyID == ourCompanyId).Select(m => new SelectListItem() { Value = m.ID.ToString(), Text = m.FullName }).ToList();
            posList = Db.VBankAccount.Where(x => x.OurCompanyID == ourCompanyId && x.AccountTypeID == 2).Select(m => new SelectListItem() { Value = m.ID.ToString(), Text = m.AccountName }).ToList(); // TODO: AccountTypeID == 2 olmasının sebebi POS işlemlerinden dolayı
            cityList = Db.VCity.Where(x => x.OurCompanyID == ourCompanyId).Select(m => new SelectListItem() { Value = m.ID.ToString(), Text = m.CityName }).ToList();

            OurCompanyModel getModel = new OurCompanyModel()
            {
                Currency = getOurCompany.Currency,
                SelectList = list,
                PriceCategoryList = priceList,
                MallList = mallList,
                PosList = posList,
                CityList = cityList
            };

            return Json(getModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public ActionResult EditLocation(FormLocation location)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();

            if (location != null)
            {
                DateTime daterecord = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                var isLocation = Db.Location.FirstOrDefault(x => x.LocationID == location.LocationID && x.LocationUID == location.LocationUID);

                if (isLocation != null)
                {
                    var isCheck = Db.Location.Where(x => (x.LocationName.Trim().ToUpper() == location.LocationName || x.SortBy.Trim().ToUpper() == location.SortBy.Trim().ToUpper()) && x.LocationID != isLocation.LocationID).ToList();
                    var getCity = Db.City.FirstOrDefault(x => x.ID == location.CityID);

                    if (isCheck.Count == 0)
                    {
                        if (isLocation != null)
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
                                    Weight = isLocation.Weight,
                                    CityID = isLocation.CityID,
                                    CountryID = isLocation.CountryID,
                                    StateID = isLocation.StateID,
                                    TaxRate = isLocation.TaxRate
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
                                isLocation.Description = location.Description;
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
                                isLocation.CityID = location.CityID;
                                isLocation.CountryID = getCity?.CountryID;
                                isLocation.StateID = getCity?.StateID;
                                isLocation.EnforcedWarning = location.EnforcedWarning;
                                isLocation.TaxRate = location.TaxRate;
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
                    }
                    else
                    {
                        var message = "";
                        if (isCheck.Count > 0 && isCheck != null)
                        {
                            foreach (var item in isCheck)
                            {
                                message += $"{item.LocationName} {item.SortBy} <br/>";
                            }
                        }

                        model.Result.IsSuccess = false;
                        model.Result.Message = $"{message} benzer kayıtlar bulundu.";

                        TempData["checkLocation"] = location;
                        TempData["result"] = model.Result;

                        return RedirectToAction("Edit", "Location", new { id = location.LocationUID });
                    }
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "İşlem sırasında hata oluştu. Lütfen bilgileri kontrol ediniz";

                TempData["result"] = model.Result;

                return RedirectToAction("Edit", "Location", new { id = location.LocationUID });
            }

            return RedirectToAction("Index", "Location");
        }

        [AllowAnonymous]
        public ActionResult Add()
        {
            LocationControlModel model = new LocationControlModel();
            model.OurCompanyList = Db.OurCompany.ToList();
            model.CityList = Db.VCity.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.MallList = Db.Mall.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationTypeList = Db.LocationType.Where(x => x.IsActive == true).ToList();
            model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 2).ToList(); // TODO: AccountTypeID == 2 olmasının sebebi POS işlemlerinden dolayı

            if (TempData["checkLocation"] != null)
            {
                model.CheckLocation = TempData["checkLocation"] as FormLocation;

                var ticketTypeId = Db.LocationType.FirstOrDefault(x => x.ID == model.CheckLocation.LocationTypeID).TicketTypeID;
                model.PriceCategoryList = model.PriceCategoryList.Where(x => x.TicketTypeID == ticketTypeId).ToList();
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [AllowAnonymous]
        public ActionResult AddLocation(FormLocation location)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();
            Guid locationUID = Guid.Empty;
            var isLocation = Db.Location.FirstOrDefault(x => x.LocationName.Trim().ToUpper() == location.LocationName.Trim().ToUpper() || x.SortBy.Trim().ToUpper() == location.SortBy.Trim().ToUpper());
            var getCity = Db.City.FirstOrDefault(x => x.ID == location.CityID);

            if (location != null && isLocation == null)
            {
                try
                {
                    locationUID = Guid.NewGuid();

                    Location locationModel = new Location()
                    {
                        OurCompanyID = location.OurCompany,
                        CityID = location.CityID,
                        CountryID = getCity.CountryID,
                        StateID = getCity.StateID,
                        Currency = location.Currency,
                        Description = location.Description,
                        EnforcedWarning = location.EnforcedWarning,
                        IP = location.IP,
                        IsActive = location.IsActive == "1" ? true : false,
                        IsHaveOperator = location.IsHaveOperator == "1" ? true : false,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        LocalDate = DateTime.UtcNow.AddHours(location.Timezone ?? 0).Date,
                        LocalDateTime = DateTime.UtcNow.AddHours(location.Timezone ?? 0),
                        LocationCode = location.LocationCode,
                        LocationName = location.LocationName,
                        LocationTypeID = location.LocationTypeID,
                        MallID = location.MallID,
                        MapURL = location.MapURL,
                        POSAccountID = location.POSAccountID,
                        PriceCatID = location.PriceCatID,
                        RecordDate = DateTime.UtcNow.AddHours(location.Timezone ?? 0),
                        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                        RecordIP = OfficeHelper.GetIPAddress(),
                        SortBy = location.SortBy,
                        Timezone = location.Timezone,
                        LocationUID = locationUID
                    };

                    Db.Location.Add(locationModel);
                    Db.SaveChanges();
                    #region LocationPeriodsInsert
                    LocationPeriods locationPeriods = new LocationPeriods()
                    {
                        OurCompanyID = locationModel.OurCompanyID,
                        LocationID = locationModel.LocationID,
                        ContractStartDate = locationModel.LocalDate,
                        RecordDate = DateTime.UtcNow.AddHours(locationModel.Timezone ?? 0),
                        RecordIP = OfficeHelper.GetIPAddress(),
                        RecordedEmployeeID = model.Authentication.ActionEmployee.EmployeeID
                    };
                    Db.LocationPeriods.Add(locationPeriods);
                    #endregion
                    #region CashInsert
                    OfficeHelper.AddCashes(locationModel.LocationID);
                    #endregion
                    #region PriceCategoryInsert
                    LocationPriceCategory priceCat = new LocationPriceCategory()
                    {
                        LocationID = locationModel?.LocationID,
                        PriceCategoryID = locationModel?.PriceCatID,
                        StartDate = DateTime.UtcNow.AddHours(locationModel?.Timezone ?? 0),
                        RecordDate = DateTime.UtcNow.AddHours(locationModel?.Timezone ?? 0),
                        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                        RecordIP = OfficeHelper.GetIPAddress()
                    };
                    Db.LocationPriceCategory.Add(priceCat);
                    #endregion

                    Db.SaveChanges();

                    #region ResultMessage
                    model.Result.IsSuccess = true;
                    model.Result.Message = $"{locationModel.LocationName} Lokasyonu eklendi.";
                    #endregion
                    #region AddLog
                    OfficeHelper.AddApplicationLog("Office", "Location", "Insert", locationModel.LocationID.ToString(), "Location", "AddLocation", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(location?.Timezone ?? 0), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, locationModel);
                    #endregion
                }
                catch (Exception ex)
                {
                    model.Result.Message = ex.Message;
                    model.Result.IsSuccess = false;
                }

                TempData["result"] = model.Result;
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"{location.LocationName} {location.SortBy} benzer bir kayıt vardır.";

                TempData["checkLocation"] = location;
                TempData["result"] = model.Result;

                return RedirectToAction("Add", "Location");
            }

            return RedirectToAction("Detail", "Location", new { id = locationUID });
        }
        #endregion
        #region PriceCategory
        [AllowAnonymous]
        public ActionResult PriceCategory(Guid id)
        {
            LocationControlModel model = new LocationControlModel();

            if (id == Guid.Empty)
            {
                return RedirectToAction("Index");
            }

            model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationUID == id);

            if (model.LocationModel != null)
            {
                model.LogList = Db.ApplicationLog.Where(x => x.Modul == "LocationPriceCategor" && x.ProcessID == model.LocationModel.LocationID.ToString()).ToList();
                model.EmployeeLocationList = Db.GetLocationEmployees(model.LocationModel.LocationID).Select(x => new LocationEmployeeModel()
                {
                    EmployeeID = x.EmployeeID,
                    EmployeeUID = x.EmployeeUID ?? Guid.Empty,
                    FullName = x.FullName,
                    Active = x.Active ?? false,
                    PositionName = x.PositionName
                }).ToList();
                model.LocationPriceLastList = Db.GetLocationPrice(model.LocationModel.LocationID,model.LocationModel.LocalDateTime).ToList();
                model.LocationPriceCategoryList = Db.VLocationPriceCategory.Where(x => x.LocationID == model.LocationModel.LocationID).ToList();
                model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.TicketTypeID == model.LocationModel.TicketTypeID).ToList();
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

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult EditPriceCat(int? ID)
        {
            Result result = new Result();

            LocationControlModel model = new LocationControlModel();

            if (ID != null)
            {
                model.LocationPriceCategory = Db.VLocationPriceCategory.FirstOrDefault(x => x.ID == ID);
                model.LocationPriceCategoryID = ID ?? 0;
                if (model.LocationPriceCategory != null)
                {
                    model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationID == model.LocationPriceCategory.LocationID);
                    model.PriceCategoryList = Db.VPriceCategory.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.TicketTypeID == model.LocationModel.TicketTypeID).ToList();
                }
            }
            else
            {
                result.Message = $"Fiyat kategorisi bilgisi boş olamaz";
            }

            TempData["result"] = result;

            model.Result = result;

            return PartialView("_PartialPriceCatEdit", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult CreatePriceCat(FormLocationPriceCategory getModel)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();

            if (getModel != null)
            {
                if (getModel.LocationID > 0)
                {
                    try
                    {
                        #region AddModel
                        LocationPriceCategory locationPriceCategory = new LocationPriceCategory()
                        {
                            LocationID = getModel.LocationID,
                            PriceCategoryID = getModel.PriceCategoryID,
                            StartDate = getModel.StartDate,
                            RecordDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 0),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = OfficeHelper.GetIPAddress()
                        };
                        #endregion

                        Db.LocationPriceCategory.Add(locationPriceCategory);
                        Db.SaveChanges();

                        Db.CheckLocationPriceCategory(getModel.LocationID);

                        #region ResultMessage
                        model.Result.IsSuccess = true;
                        model.Result.Message = $"Lokasyon fiyat kategorisi eklendi.";
                        #endregion
                        #region AddLog
                        OfficeHelper.AddApplicationLog("Office", "LocationPriceCategory", "Insert", locationPriceCategory.LocationID.ToString(), "Location", "CreatePriceCat", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 0), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, locationPriceCategory);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        model.Result.Message = ex.Message;
                        model.Result.IsSuccess = false;
                    }

                    TempData["result"] = model.Result;

                    model.LocationPriceCategoryList = Db.VLocationPriceCategory.Where(x => x.LocationID == getModel.LocationID).ToList();
                }
            }

            return PartialView("_PartialLocationPriceList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult UpdatePriceCat(FormLocationPriceCategory getModel)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();

            if (getModel != null)
            {
                if (getModel.LocationID > 0)
                {
                    DateTime daterecord = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value);
                    var isLocationPrice = Db.LocationPriceCategory.FirstOrDefault(x => x.ID == getModel.ID);

                    if (isLocationPrice != null)
                    {
                        var isCheck = Db.LocationPriceCategory.Where(x => (x.PriceCategoryID == getModel.PriceCategoryID) && x.ID != isLocationPrice.ID && x.LocationID == getModel.LocationID).ToList();

                        if (isCheck.Count == 0)
                        {
                            if (isLocationPrice != null)
                            {
                                try
                                {
                                    #region SelfModel
                                    LocationPriceCategory self = new LocationPriceCategory()
                                    {
                                        LocationID = isLocationPrice.LocationID,
                                        PriceCategoryID = isLocationPrice.PriceCategoryID,
                                        StartDate = isLocationPrice.StartDate,
                                        RecordDate = isLocationPrice.RecordDate,
                                        RecordEmployeeID = isLocationPrice.RecordEmployeeID,
                                        RecordIP = isLocationPrice.RecordIP
                                    };
                                    #endregion
                                    #region UpdateModel
                                    isLocationPrice.LocationID = getModel.LocationID;
                                    isLocationPrice.PriceCategoryID = getModel.PriceCategoryID;
                                    isLocationPrice.StartDate = getModel.StartDate;
                                    isLocationPrice.UpdateDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 0);
                                    isLocationPrice.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                                    isLocationPrice.UpdateIP = OfficeHelper.GetIPAddress();
                                    #endregion

                                    Db.SaveChanges();

                                    Db.CheckLocationPriceCategory(getModel.LocationID);

                                    #region ResultMessage
                                    model.Result.IsSuccess = true;
                                    model.Result.Message = $"Lokasyon fiyat kategorisi güncellendi.";
                                    #endregion
                                    #region AddLog
                                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<LocationPriceCategory>(self, isLocationPrice, OfficeHelper.getIgnorelist());
                                    OfficeHelper.AddApplicationLog("Office", "LocationPriceCategory", "Update", isLocationPrice.LocationID.ToString(), "Location", "UpdatePriceCat", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 0), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                                    #endregion
                                }
                                catch (Exception ex)
                                {
                                    model.Result.Message = ex.Message;
                                    model.Result.IsSuccess = false;
                                }

                                TempData["result"] = model.Result;
                            }
                        }
                        else
                        {
                            model.Result.IsSuccess = false;
                            model.Result.Message = $"Benzer kayıtlar bulundu.";

                            //TempData["checkLocation"] = location;
                            TempData["result"] = model.Result;

                            //return RedirectToAction("Edit", "Location", new { id = location.LocationUID });
                        }
                    }

                    model.LocationPriceCategoryList = Db.VLocationPriceCategory.Where(x => x.LocationID == getModel.LocationID).ToList();
                }
            }

            return PartialView("_PartialLocationPriceList", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult DeletePriceCat(int? ID)
        {
            LocationControlModel model = new LocationControlModel();
            model.Result = new Result();

            if (ID != null && ID > 0)
            {
                try
                {
                    var isLocationPriceCategory = Db.LocationPriceCategory.FirstOrDefault(x => x.ID == ID);

                    if (isLocationPriceCategory != null)
                    {
                        #region ResultMessage
                        model.Result.IsSuccess = true;
                        model.Result.Message = $"Lokasyon fiyat kategorisi silindi.";
                        #endregion
                        #region AddLog
                        OfficeHelper.AddApplicationLog("Office", "LocationPriceCategory", "Delete", isLocationPriceCategory.LocationID.ToString(), "Location", "DeletePriceCat", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 0), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isLocationPriceCategory);
                        #endregion

                        Db.LocationPriceCategory.Remove(isLocationPriceCategory);
                        Db.SaveChanges();

                        Db.CheckLocationPriceCategory(isLocationPriceCategory.LocationID);

                        model.LocationPriceCategoryList = Db.VLocationPriceCategory.Where(x => x.LocationID == isLocationPriceCategory.LocationID).ToList();
                    }
                }
                catch (Exception ex)
                {
                    model.Result.Message = ex.Message;
                    model.Result.IsSuccess = false;
                }

                TempData["result"] = model.Result;
            }

            return PartialView("_PartialLocationPriceList", model);
        }

        #endregion
        #region Schedule
        [AllowAnonymous]
        public ActionResult Schedule(Guid? id, string week)
        {
            LocationControlModel model = new LocationControlModel();

            if (id != null)
            {
                model.LocationModel = Db.VLocation.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.LocationUID == id);
            }

            #region WeekDate
            var _date = DateTime.Now.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(week))
            {
                var weekparts = week.Split('-');
                int _year = Convert.ToInt32(weekparts[0]);
                int _week = Convert.ToInt32(weekparts[1]);
                datekey = Db.DateList.Where(x => x.WeekYear == _year && x.WeekNumber == _week).OrderBy(x => x.DateKey).FirstOrDefault();
            }

            string weekcode = $"{datekey.WeekYear}-{datekey.WeekNumber}";
            var weekdatekeys = Db.DateList.Where(x => x.WeekYear == datekey.WeekYear && x.WeekNumber == datekey.WeekNumber).ToList();

            model.WeekCode = weekcode;

            model.CurrentDate = datekey;
            model.WeekList = weekdatekeys;
            model.FirstWeekDay = weekdatekeys.OrderBy(x => x.DateKey).FirstOrDefault();
            model.LastWeekDay = weekdatekeys.OrderByDescending(x => x.DateKey).FirstOrDefault();

            var prevdate = model.FirstWeekDay.DateKey.AddDays(-1).Date;
            var nextdate = model.LastWeekDay.DateKey.AddDays(1).Date;

            var prevday = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var nextday = Db.DateList.FirstOrDefault(x => x.DateKey == nextdate);

            model.NextWeekCode = $"{nextday.WeekYear}-{nextday.WeekNumber}";
            model.PrevWeekCode = $"{prevday.WeekYear}-{prevday.WeekNumber}";

            List<DateTime> datelist = model.WeekList.Select(x => x.DateKey).Distinct().ToList(); 
            #endregion
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

            return View(model);
        }
        #endregion
    }

    #region OurCompanyModel
    //TODO: SelectListItem MVC'den türediği için başka bir sınıfın içerisine alamadığımızdan dolayı OurCompanyModel sınıfı burada bulunmaktadır.
    public class OurCompanyModel
    {
        public List<SelectListItem> SelectList { get; set; }
        public List<SelectListItem> PriceCategoryList { get; set; }
        public List<SelectListItem> MallList { get; set; }
        public List<SelectListItem> PosList { get; set; }
        public List<SelectListItem> CityList { get; set; }
        public string Currency { get; set; }
    }
    #endregion
}