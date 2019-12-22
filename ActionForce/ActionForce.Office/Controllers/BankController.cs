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
    public class BankController : BaseController
    {
        // GET: Bank
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCollections = Db.VDocumentPosCollection.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosCollections = model.PosCollections.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            if (model.Filters.BankAccountID > 0)
            {
                model.PosCollections = model.PosCollections.Where(x => x.BankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? locationId, int? BankAccountID, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.BankAccountID = BankAccountID;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Index", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosCollection(NewPosCollect posCollect)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                
                var refID = string.IsNullOrEmpty(posCollect.ReferanceID);
                var exchange = OfficeHelper.GetExchange(docDate);

                PosCollection pos = new PosCollection();

                pos.ActinTypeID = actType.ID;
                pos.ActionTypeName = actType.Name;
                pos.Amount = amount;
                pos.BankAccountID = posCollect.BankAccountID;
                pos.Currency = currency;
                pos.DocumentDate = docDate;
                pos.Description = posCollect.Description;
                pos.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                pos.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                pos.LocationID = posCollect.LocationID;
                pos.OurCompanyID = location.OurCompanyID;
                pos.TimeZone = timezone;
                pos.ReferanceID = refID == false ? Convert.ToInt64(posCollect.ReferanceID) : (long?)null;
                pos.TerminalID = posTerminal != null ? posTerminal.TerminalID?.ToString() : "";

                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddPosCollection(pos, model.Authentication);
                

            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Bank");
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.Detail = Db.VDocumentPosCollection.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.Detail.ID.ToString()).ToList();
            

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosCollection(NewPosCollect posCollect)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);
                var exchanges = posCollect.ExchangeRate;
                var isDate = DateTime.Now.Date;
                var isKasa = posCollect.BankAccountID;
                int? locId = location.LocationID;
                var isCash = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == posCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        locId = isCash.LocationID;
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.BankAccountID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentPosCollections self = new DocumentPosCollections()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            FromCustomerID = isCash.FromCustomerID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            BankAccountID = isCash.BankAccountID,
                            TerminalID = isCash.TerminalID,
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.LocationID = posCollect.LocationID;
                        isCash.Date = docDate;
                        isCash.BankAccountID = posCollect.BankAccountID;
                        isCash.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Currency = posCollect.Currency;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(posCollect.ExchangeRate.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isKasa && x.LocationID == locId && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isCash.LocationID;
                            cashaction.Collection = isCash.Amount;
                            cashaction.Currency = posCollect.Currency;
                            cashaction.BankAccountID = posCollect.BankAccountID;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki pos tahsilatı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", isCash.ID.ToString(), "Bank", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki pos tahsilatı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Detail", new { id = posCollect.UID });
            //return RedirectToAction("Index", "Bank");
        }
        
        [AllowAnonymous]
        public ActionResult DeletePosCollection(int? id)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (id != null)
            {
                
                var isCash = Db.DocumentPosCollections.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentPosCollections self = new DocumentPosCollections()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            FromCustomerID = isCash.FromCustomerID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            BankAccountID = isCash.BankAccountID,
                            TerminalID = isCash.TerminalID,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.BankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki pos tahsilatı başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos tahsilatı iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Bank");
        }



        [AllowAnonymous]
        public ActionResult PosCancel(int? locationId)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCancel = Db.VDocumentPosCancel.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosCancel = model.PosCancel.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            if (model.Filters.BankAccountID > 0)
            {
                model.PosCancel = model.PosCancel.Where(x => x.FromBankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterCancel(int? locationId, int? BankAccountID, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();
            model.BankAccountID = BankAccountID;
            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("PosCancel", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosCancel(NewPosCancel posCollect)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);
                var refID = string.IsNullOrEmpty(posCollect.ReferanceID);

                var exchange = OfficeHelper.GetExchange(docDate);

                PosCancel pos = new PosCancel();

                pos.ActinTypeID = actType.ID;
                pos.ActionTypeName = actType.Name;
                pos.Amount = amount;
                pos.FromBankAccountID = posCollect.BankAccountID;
                pos.Currency = currency;
                pos.DocumentDate = docDate;
                pos.Description = posCollect.Description;
                pos.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                pos.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                pos.LocationID = posCollect.LocationID;
                pos.OurCompanyID = location.OurCompanyID;
                pos.TimeZone = timezone;
                pos.ReferanceID = refID == false ? Convert.ToInt64(posCollect.ReferanceID) : (long?)null;
                pos.TerminalID = posTerminal != null ? posTerminal.TerminalID?.ToString() : "";

                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddPosCancel(pos, model.Authentication);
                

            }

            TempData["result"] = result;

            return RedirectToAction("PosCancel", "Bank");
        }

        [AllowAnonymous]
        public ActionResult PosCancelDetail(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCancelDetail = Db.VDocumentPosCancel.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "PosCancel" && x.Environment == "Office" && x.ProcessID == model.PosCancelDetail.ID.ToString()).ToList();


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosCancel(NewPosCancel posCollect)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);
                var exchanges = posCollect.ExchangeRate;
                var isDate = DateTime.Now.Date;
                var isKasa = posCollect.BankAccountID;
                int? locId = location.LocationID;
                var isCash = Db.DocumentPosCancel.FirstOrDefault(x => x.UID == posCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        locId = isCash.LocationID;
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.FromBankAccountID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentPosCancel self = new DocumentPosCancel()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            ToCustomerID = isCash.ToCustomerID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            FromBankAccountID = isCash.FromBankAccountID,
                            TerminalID = isCash.TerminalID,
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.LocationID = posCollect.LocationID;
                        isCash.Date = docDate;
                        isCash.FromBankAccountID = posCollect.BankAccountID;
                        isCash.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Currency = posCollect.Currency;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(posCollect.ExchangeRate.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isKasa && x.LocationID == locId && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isCash.LocationID;
                            cashaction.Payment = isCash.Amount;
                            cashaction.Currency = posCollect.Currency;
                            cashaction.BankAccountID = posCollect.BankAccountID;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki pos iptali başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCancel>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", isCash.ID.ToString(), "Bank", "PosCancel", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki pos iptali güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("PosCancelDetail", new { id = posCollect.UID });
        }

        [AllowAnonymous]
        public ActionResult DeletePosCancel(int? id)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (id != null)
            {

                var isCash = Db.DocumentPosCancel.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentPosCancel self = new DocumentPosCancel()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            ToCustomerID = isCash.ToCustomerID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            FromBankAccountID = isCash.FromBankAccountID,
                            TerminalID = isCash.TerminalID,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki pos iptali başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos iptali iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;

            return RedirectToAction("PosCancel", "Bank");
        }



        [AllowAnonymous]
        public ActionResult PosRefund(int? locationId)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosRefund = Db.VDocumentPosRefund.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosRefund = model.PosRefund.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            if (model.Filters.BankAccountID > 0)
            {
                model.PosRefund = model.PosRefund.Where(x => x.FromBankAccountID == model.Filters.BankAccountID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterRefund(int? locationId, int? BankAccountID, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();
            model.BankAccountID = BankAccountID;
            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("PosRefund", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosRefund(NewPosReturn posCollect)
        {
            Result<DocumentPosRefund> result = new Result<DocumentPosRefund>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);

                var refID = string.IsNullOrEmpty(posCollect.ReferanceID);

                var exchange = OfficeHelper.GetExchange(docDate);

                PosRefund pos = new PosRefund();

                pos.ActinTypeID = actType.ID;
                pos.ActionTypeName = actType.Name;
                pos.Amount = amount;
                pos.FromBankAccountID = posCollect.BankAccountID;
                pos.Currency = currency;
                pos.DocumentDate = docDate;
                pos.Description = posCollect.Description;
                pos.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                pos.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                pos.LocationID = posCollect.LocationID;
                pos.OurCompanyID = location.OurCompanyID;
                pos.TimeZone = timezone;
                pos.ReferanceID = refID == false ? Convert.ToInt64(posCollect.ReferanceID) : (long?)null;
                pos.TerminalID = posTerminal != null ? posTerminal.TerminalID?.ToString() : "";

                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddPosRefund(pos, model.Authentication);
                

            }

            TempData["result"] = result;

            return RedirectToAction("PosRefund", "Bank");
        }

        [AllowAnonymous]
        public ActionResult PosRefundDetail(Guid? id)
        {
            BankControlModel model = new BankControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<BankActions> ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.BankAccountList = Db.BankAccount.Where(x => x.AccountTypeID == 2 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosRefundDetail = Db.VDocumentPosRefund.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "PosRefund" && x.Environment == "Office" && x.ProcessID == model.PosRefundDetail.ID.ToString()).ToList();


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPosRefund(NewPosReturn posCollect)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCollect != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCollect.FromID.Substring(1, posCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);
                var exchanges = posCollect.ExchangeRate;
                var isDate = DateTime.Now.Date;
                var isKasa = posCollect.BankAccountID;
                int? locId = location.LocationID;
                var isCash = Db.DocumentPosRefund.FirstOrDefault(x => x.UID == posCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.FromBankAccountID);
                        locId = isCash.LocationID;
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentPosRefund self = new DocumentPosRefund()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            ToCustomerID = isCash.ToCustomerID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            FromBankAccountID = isCash.FromBankAccountID,
                            TerminalID = isCash.TerminalID,
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.LocationID = posCollect.LocationID;
                        isCash.Date = docDate;
                        isCash.FromBankAccountID = posCollect.BankAccountID;
                        isCash.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Currency = posCollect.Currency;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(posCollect.ExchangeRate.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isKasa && x.LocationID == locId && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.LocationID = isCash.LocationID;
                            cashaction.Payment = isCash.Amount;
                            cashaction.Currency = posCollect.Currency;
                            cashaction.BankAccountID = posCollect.BankAccountID;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki pos iades, başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosRefund>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", isCash.ID.ToString(), "Bank", "PosRefund", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki pos iptali güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Update", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("PosRefundDetail", new { id = posCollect.UID });
        }

        [AllowAnonymous]
        public ActionResult DeletePosRefund(int? id)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (id != null)
            {

                var isCash = Db.DocumentPosRefund.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentPosRefund self = new DocumentPosRefund()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            ToCustomerID = isCash.ToCustomerID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            FromBankAccountID = isCash.FromBankAccountID,
                            TerminalID = isCash.TerminalID,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki pos iadesi başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki pos iadesi iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;

            return RedirectToAction("PosRefund", "Bank");
        }

        [AllowAnonymous]
        public ActionResult Current(int? locationId)
        {
            ActionControlModel model = new ActionControlModel();

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.LocationID = locationId != null ? locationId : Db.Location.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).LocationID;
                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.bankAccount = Db.BankAccount.ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.ActionList = Db.VCashBankActions.Where(x => x.LocationID == model.Filters.LocationID && x.ActionDate >= model.Filters.DateBegin && x.ActionDate <= model.Filters.DateEnd).OrderBy(x => x.ActionDate).ToList();

            var balanceData = Db.VCashBankActions.Where(x => x.LocationID == model.Filters.LocationID && x.ActionDate < model.Filters.DateBegin).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "TRL" && x.Module == "Bank").Sum(x => x.CardAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "USD" && x.Module == "Bank").Sum(x => x.CardAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "EUR" && x.Module == "Bank").Sum(x => x.CardAmount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Bank",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }


            List<TotalModel> footerTotals = new List<TotalModel>(); // ilk başta header ile footer aynı olur ekranda foreach içinde footer değişir. 

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "TRL").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "USD").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "EUR").Total
            });

            model.FooterTotals = footerTotals;


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filters(int? locationId, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Current", "Bank");
        }
    }
}