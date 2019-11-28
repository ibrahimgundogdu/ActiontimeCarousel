using ActionForce.Entity;
using System;
using System.Collections.Generic;
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
            model.BankAccountList = Db.BankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCollections = Db.VDocumentPosCollection.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosCollections = model.PosCollections.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("Index", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosCollection(NewPosCollect posCollect)
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
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", ","));
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);


                try
                {
                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                    DocumentPosCollections newPosColl = new DocumentPosCollections();

                    newPosColl.ActionTypeID = actType.ID;
                    newPosColl.ActionTypeName = actType.Name;
                    newPosColl.Amount = amount;
                    newPosColl.Currency = currency;
                    newPosColl.Date = docDate;
                    newPosColl.Description = posCollect.Description;
                    newPosColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "PC");
                    newPosColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    newPosColl.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    newPosColl.IsActive = true;
                    newPosColl.LocationID = posCollect.LocationID;
                    newPosColl.OurCompanyID = location.OurCompanyID;
                    newPosColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                    newPosColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newPosColl.RecordIP = OfficeHelper.GetIPAddress();
                    newPosColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newPosColl.ExchangeRate;
                    newPosColl.SystemCurrency = ourcompany.Currency;
                    newPosColl.EnvironmentID = 2;
                    newPosColl.BankAccountID = posCollect.BankAccountID;
                    newPosColl.TerminalID = posTerminal.TerminalNumber != "" ? posTerminal.TerminalNumber : null;

                    Db.DocumentPosCollections.Add(newPosColl);
                    Db.SaveChanges();

                    // cari hesap işlemesi
                    OfficeHelper.AddBankAction(newPosColl.LocationID, null, newPosColl.BankAccountID, posTerminal.ID, newPosColl.ActionTypeID, newPosColl.Date, newPosColl.ActionTypeName, newPosColl.ID, newPosColl.Date, newPosColl.DocumentNumber, newPosColl.Description, 1, newPosColl.Amount, 0, newPosColl.Currency, null, null, newPosColl.RecordEmployeeID, newPosColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Pos Tahsilatı başarı ile eklendi";

                    // log atılır
                    OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", newPosColl.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newPosColl);

                }
                catch (Exception ex)
                {

                    result.Message = $"Pos Tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);

                }

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
            model.BankAccountList = Db.BankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.Detail = Db.VDocumentPosCollection.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Bank" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.Detail.ID.ToString()).ToList();
            

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

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
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", ","));
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);

                var isCash = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == posCollect.UID);
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


                        isCash.BankAccountID = isCash.BankAccountID;
                        isCash.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isCash.BankAccountID && x.LocationID == isCash.LocationID && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isCash.Amount;
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

            return RedirectToAction("Index", "Bank");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult DeletePosCollection(NewPosCollect posCollect)
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
                var amount = Convert.ToDouble(posCollect.Amount.Replace(".", ","));
                var currency = posCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCollect.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCollect.DocumentDate).Date;
                }
                //var cash = OfficeHelper.GetCash(posCollect.LocationID, posCollect.Currency);
                var isCash = Db.DocumentPosCollections.FirstOrDefault(x => x.UID == posCollect.UID);
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

                        OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.BankAccountID, posTerminal.ID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki pos tahsilatı başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPosCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Bank", "Remove", isCash.ID.ToString(), "Bank", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki pos tahsilatı iptal edilemedi : {ex.Message}";
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
            model.BankAccountList = Db.BankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosCancel = Db.VDocumentPosCancel.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosCancel = model.PosCancel.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosCancel(NewPosCancel posCancel)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posCancel != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posCancel.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posCancel.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posCancel.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posCancel.FromID.Substring(1, posCancel.FromID.Length - 1));
                var amount = Convert.ToDouble(posCancel.Amount.Replace(".", ","));
                var currency = posCancel.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posCancel.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posCancel.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posCancel.DocumentDate).Date;
                }



                try
                {
                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                    DocumentPosCancel newPosCancel = new DocumentPosCancel();

                    newPosCancel.ActionTypeID = actType.ID;
                    newPosCancel.ActionTypeName = actType.Name;
                    newPosCancel.Amount = amount;
                    newPosCancel.Currency = currency;
                    newPosCancel.Date = docDate;
                    newPosCancel.Description = posCancel.Description;
                    newPosCancel.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "PC");
                    newPosCancel.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    newPosCancel.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    newPosCancel.IsActive = true;
                    newPosCancel.LocationID = posCancel.LocationID;
                    newPosCancel.OurCompanyID = location.OurCompanyID;
                    newPosCancel.RecordDate = DateTime.UtcNow.AddHours(timezone);
                    newPosCancel.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newPosCancel.RecordIP = OfficeHelper.GetIPAddress();
                    newPosCancel.SystemAmount = ourcompany.Currency == currency ? amount : amount * newPosCancel.ExchangeRate;
                    newPosCancel.SystemCurrency = ourcompany.Currency;
                    newPosCancel.EnvironmentID = 2;
                    newPosCancel.FromBankAccountID = posCancel.BankAccountID;
                    newPosCancel.TerminalID = posTerminal.TerminalNumber;

                    Db.DocumentPosCancel.Add(newPosCancel);
                    Db.SaveChanges();

                    OfficeHelper.AddBankAction(newPosCancel.LocationID, null, newPosCancel.FromBankAccountID, posTerminal.ID, newPosCancel.ActionTypeID, newPosCancel.Date, newPosCancel.ActionTypeName, newPosCancel.ID, newPosCancel.Date, newPosCancel.DocumentNumber, newPosCancel.Description, -1, 0, newPosCancel.Amount, newPosCancel.Currency, null, null, newPosCancel.RecordEmployeeID, newPosCancel.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Pos Tahsilatı başarı ile eklendi";

                    // log atılır
                    OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", newPosCancel.ID.ToString(), "Bank", "PosCancel", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newPosCancel);

                }
                catch (Exception ex)
                {

                    result.Message = $"Pos Tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosCancel", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);

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
            model.BankAccountList = Db.BankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PosRefund = Db.VDocumentPosRefund.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.PosRefund = model.PosRefund.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPosRefund(NewPosCancel posReturn)
        {
            Result<BankActions> result = new Result<BankActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            BankControlModel model = new BankControlModel();

            if (posReturn != null)
            {
                var actType = Db.BankActionType.FirstOrDefault(x => x.ID == posReturn.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == posReturn.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = posReturn.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(posReturn.FromID.Substring(1, posReturn.FromID.Length - 1));
                var amount = Convert.ToDouble(posReturn.Amount.Replace(".", ","));
                var currency = posReturn.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == posReturn.LocationID && x.IsActive == true && x.IsMaster == true);

                if (DateTime.TryParse(posReturn.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(posReturn.DocumentDate).Date;
                }



                try
                {
                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                    DocumentPosRefund newPosRefund = new DocumentPosRefund();

                    newPosRefund.ActionTypeID = actType.ID;
                    newPosRefund.ActionTypeName = actType.Name;
                    newPosRefund.Amount = amount;
                    newPosRefund.Currency = currency;
                    newPosRefund.Date = docDate;
                    newPosRefund.Description = posReturn.Description;
                    newPosRefund.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "PC");
                    newPosRefund.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    newPosRefund.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    newPosRefund.IsActive = true;
                    newPosRefund.LocationID = posReturn.LocationID;
                    newPosRefund.OurCompanyID = location.OurCompanyID;
                    newPosRefund.RecordDate = DateTime.UtcNow.AddHours(timezone);
                    newPosRefund.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newPosRefund.RecordIP = OfficeHelper.GetIPAddress();
                    newPosRefund.SystemAmount = ourcompany.Currency == currency ? amount : amount * newPosRefund.ExchangeRate;
                    newPosRefund.SystemCurrency = ourcompany.Currency;
                    newPosRefund.EnvironmentID = 2;
                    newPosRefund.FromBankAccountID = posReturn.BankAccountID;
                    newPosRefund.TerminalID = posTerminal.TerminalNumber;

                    Db.DocumentPosRefund.Add(newPosRefund);
                    Db.SaveChanges();

                    OfficeHelper.AddBankAction(newPosRefund.LocationID, null, newPosRefund.FromBankAccountID, posTerminal.ID, newPosRefund.ActionTypeID, newPosRefund.Date, newPosRefund.ActionTypeName, newPosRefund.ID, newPosRefund.Date, newPosRefund.DocumentNumber, newPosRefund.Description, -1, 0, newPosRefund.Amount, newPosRefund.Currency, null, null, newPosRefund.RecordEmployeeID, newPosRefund.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Pos Tahsilatı başarı ile eklendi";

                    // log atılır
                    OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", newPosRefund.ID.ToString(), "Bank", "PosRefund", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newPosRefund);

                }
                catch (Exception ex)
                {

                    result.Message = $"Pos Tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Bank", "Insert", "-1", "Bank", "PosRefund", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty,null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("PosRefund", "Bank");
        }
    }
}