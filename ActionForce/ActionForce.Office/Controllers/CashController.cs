using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class CashController : BaseController
    {
        // GET: Cash
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashCollections = Db.VDocumentCashCollections.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashCollections = model.CashCollections.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

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

            return RedirectToAction("Index", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashCollection(NewCashCollect cashCollect)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashCollect.LocationID, cashCollect.Currency);

                //var isCash = Db.DocumentCashCollections.FirstOrDefault(x => x.UID == cashCollect.UID);

                try
                {
                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                    DocumentCashCollections newCashColl = new DocumentCashCollections();

                    newCashColl.ActionTypeID = actType.ID;
                    newCashColl.ActionTypeName = actType.Name;
                    newCashColl.Amount = amount;
                    newCashColl.CashID = cash.ID;
                    newCashColl.Currency = currency;
                    newCashColl.Date = docDate;
                    newCashColl.Description = cashCollect.Description;
                    newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "CC");
                    newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    newCashColl.FromBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                    newCashColl.FromEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                    newCashColl.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    newCashColl.IsActive = true;
                    newCashColl.LocationID = cashCollect.LocationID;
                    newCashColl.OurCompanyID = location.OurCompanyID;
                    newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                    newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                    newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                    newCashColl.SystemCurrency = ourcompany.Currency;
                    newCashColl.EnvironmentID = 2;
                    newCashColl.UID = Guid.NewGuid();

                    Db.DocumentCashCollections.Add(newCashColl);
                    Db.SaveChanges();

                    // cari hesap işlemesi
                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = $"{newCashColl.Date} tarihli { newCashColl.Amount } {newCashColl.Currency} tutarındaki kasa tahsilatı başarı ile eklendi";

                    // log atılır
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);

                }
                catch (Exception ex)
                {

                    result.Message = $"{amount} {currency} tutarındaki kasa tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }


            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashCollection(NewCashCollect cashCollect)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);



                var isCash = Db.DocumentCashCollections.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentCashCollections self = new DocumentCashCollections()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
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
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.FromBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                        isCash.FromEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
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

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.CashID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isCash.Amount;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki kasa tahsilatı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa tahsilatı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            //return RedirectToAction("CashDetail", new { id = cashCollect.UID });
            return RedirectToAction("Index", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashCollection(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {

                var isCash = Db.DocumentCashCollections.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentCashCollections self = new DocumentCashCollections()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
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
                            FromEmployeeID = isCash.FromEmployeeID,
                            FromCustomerID = isCash.FromCustomerID,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa tahsilatı iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Index", "Cash");

        }

        [AllowAnonymous]
        public ActionResult CashDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashDetail = Db.VDocumentCashCollections.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.CashDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).ToList();

            return View(model);
        }
        
        [AllowAnonymous]
        public ActionResult Sale(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashSales = Db.VDocumentTicketSales.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashSales = model.CashSales.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashSale(NewCashSale cashSale)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashSale != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSale.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSale.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSale.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSale.FromID.Substring(1, cashSale.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSale.Amount.Replace(".", ","));
                var currency = cashSale.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSale.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSale.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSale.LocationID, cashSale.Currency);

                // satış eklenir.
                try
                {
                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                    DocumentTicketSales newCashColl = new DocumentTicketSales();

                    newCashColl.ActionTypeID = actType.ID;
                    newCashColl.ActionTypeName = actType.Name;
                    newCashColl.Amount = amount;
                    newCashColl.CashID = cash.ID;
                    newCashColl.Currency = currency;
                    newCashColl.Date = docDate;
                    newCashColl.Description = cashSale.Description;
                    newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "TS");
                    newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    newCashColl.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    newCashColl.IsActive = true;
                    newCashColl.LocationID = cashSale.LocationID;
                    newCashColl.OurCompanyID = location.OurCompanyID;
                    newCashColl.RecordDate = DateTime.UtcNow.AddHours(3);
                    newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                    newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                    newCashColl.SystemCurrency = ourcompany.Currency;
                    newCashColl.PayMethodID = 1;
                    newCashColl.Quantity = cashSale.Quantity;
                    newCashColl.EnvironmentID = 2;
                    newCashColl.UID = Guid.NewGuid();

                    Db.DocumentTicketSales.Add(newCashColl);
                    Db.SaveChanges();

                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Bilet satışı başarı ile eklendi";

                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);

                }
                catch (Exception ex)
                {
                    result.Message = $"Bilet satışı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
            }

            TempData["result"] = result;

            return RedirectToAction("Sale", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashSale(NewCashSale cashCollect)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);



                var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentTicketSales self = new DocumentTicketSales()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
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
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.Quantity = isCash.Quantity;
                        isCash.PayMethodID = isCash.PayMethodID;
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

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.CashID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isCash.Amount;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki {isCash.Quantity} adet bilet satışı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSales>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Sale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki {isCash.Quantity} adet bilet satışı güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            //return RedirectToAction("CashDetail", new { id = cashCollect.UID });
            return RedirectToAction("Sale", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashSale(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {

                var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentTicketSales self = new DocumentTicketSales()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
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
                            FromCustomerID = isCash.FromCustomerID,
                            PayMethodID = isCash.PayMethodID,
                            Quantity = isCash.Quantity,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSales>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Sale", "Cash");

        }

        [AllowAnonymous]
        public ActionResult SaleDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.SaleDetail = Db.VDocumentTicketSales.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "Sale" && x.Environment == "Office" && x.ProcessID == model.SaleDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Exchange(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            var ourCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompany.CompanyID);
            model.CurrencyList = OfficeHelper.GetCurrency().Where(x => x.Code != ourCompany.Currency).ToList();

            model.CashSaleExchanges = Db.VDocumentSaleExchange.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashSaleExchanges = model.CashSaleExchanges.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashExchange(NewCashExchange cashSale, HttpPostedFileBase documentFile)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashSale != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSale.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSale.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var amount = Convert.ToDouble(cashSale.Amount.Replace(".", ","));
                var exchangerate = Convert.ToDouble(cashSale.Exchange.Replace(".", ","));

                var currencyo = cashSale.Currency;
                var currencyi = location.Currency != null ? location.Currency : ourcompany.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSale.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSale.DocumentDate).Date;
                }
                var casho = OfficeHelper.GetCash(cashSale.LocationID, currencyo);
                var cashi = OfficeHelper.GetCash(cashSale.LocationID, currencyi);

                var balance = Db.GetCashBalance(location.LocationID, casho.ID).FirstOrDefault().Value;


                // exchange eklenir.
                if (balance >= amount)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                        DocumentSaleExchange newCashColl = new DocumentSaleExchange();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;

                        newCashColl.FromCashID = casho.ID;
                        newCashColl.Amount = amount;
                        newCashColl.Currency = currencyo;

                        newCashColl.SaleExchangeRate = exchangerate;
                        newCashColl.ToCashID = cashi.ID;
                        newCashColl.ToAmount = (newCashColl.Amount * newCashColl.SaleExchangeRate);
                        newCashColl.ToCurrency = currencyi;

                        newCashColl.Date = docDate;
                        newCashColl.Description = cashSale.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXS");
                        newCashColl.ExchangeRate = currencyo == "USD" ? exchange.USDA : currencyo == "EUR" ? exchange.EURA : 1;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashSale.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(3);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.EnvironmentID = 2;
                        newCashColl.UID = Guid.NewGuid();

                        if (documentFile != null && documentFile.ContentLength > 0)
                        {
                            string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);

                            try
                            {
                                documentFile.SaveAs(Path.Combine(Server.MapPath("/Document/Exchange"), filename));
                                newCashColl.SlipDocument = filename;
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        Db.DocumentSaleExchange.Add(newCashColl);
                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(newCashColl.FromCashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);
                        OfficeHelper.AddCashAction(newCashColl.ToCashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.ToAmount, 0, newCashColl.ToCurrency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{newCashColl.Amount} {newCashColl.Currency} kasa döviz satışı başarı ile eklendi";

                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{amount} {currencyo} kasa döviz satışı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.Message = $"{amount} {currencyo} kasa döviz satışı eklenemedi. Kasa bakiyesi bu tutar için  yetersiz";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Exchange", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashExchange(NewCashExchange cashCollect, HttpPostedFileBase documentFile)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", ","));
                var currencyo = cashCollect.Currency;
                var currencyi = location.Currency != null ? location.Currency : ourcompany.Currency;
                var exchangerate = Convert.ToDouble(cashCollect.Exchange.Replace(".", ","));
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }

                var casho = OfficeHelper.GetCash(cashCollect.LocationID, currencyo);
                var cashi = OfficeHelper.GetCash(cashCollect.LocationID, currencyi);



                var isCash = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentSaleExchange self = new DocumentSaleExchange()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,

                            FromCashID = casho.ID,
                            Amount = amount,
                            Currency = currencyo,

                            SaleExchangeRate = exchangerate,
                            ToCashID = cashi.ID,
                            ToAmount = (isCash.Amount * isCash.SaleExchangeRate),
                            ToCurrency = currencyi,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            EnvironmentID = isCash.EnvironmentID
                        };
                        isCash.Amount = amount;
                        isCash.ToAmount = (isCash.Amount * isCash.SaleExchangeRate);
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = currencyo == "USD" ? exchange.USDA : currencyo == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        if (documentFile != null && documentFile.ContentLength > 0)
                        {
                            string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);

                            try
                            {
                                documentFile.SaveAs(Path.Combine(Server.MapPath("/Document/Exchange"), filename));
                                isCash.SlipDocument = filename;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();
                        
                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.FromCashID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);
                        var cashactioni = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.ToCashID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);
                        if (cashaction != null)
                        {
                            cashaction.Collection = isCash.Amount;
                            cashaction.Currency = isCash.Currency;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }
                        if (cashactioni != null)
                        {
                            cashactioni.Collection = isCash.ToAmount;
                            cashactioni.Currency = isCash.ToCurrency;
                            cashactioni.UpdateDate = isCash.UpdateDate;
                            cashactioni.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }
                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} kasa döviz satışı başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSaleExchange>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "ExchangeSale", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{amount} {currencyo} kasa döviz satışı Güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            //return RedirectToAction("CashDetail", new { id = cashCollect.UID });
            return RedirectToAction("Exchange", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashExchange(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {

                var isCash = Db.DocumentSaleExchange.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentSaleExchange self = new DocumentSaleExchange()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            ToCashID = isCash.ToCashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            ToAmount = isCash.ToAmount,
                            ToCurrency = isCash.ToCurrency,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            FromCashID = isCash.FromCashID,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();
                        
                        OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);
                        OfficeHelper.AddCashAction(isCash.ToCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, 1, -1 * isCash.ToAmount, 0, isCash.ToCurrency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} kasa döviz satışı başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSales>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} kasa döviz satışı iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Exchange", "Cash");

        }

        [AllowAnonymous]
        public ActionResult ExchangeDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.ExchangeDetail = Db.VDocumentSaleExchange.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "ExchangeSale" && x.Environment == "Office" && x.ProcessID == model.ExchangeDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Open(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashOpenSlip = Db.VDocumentCashOpen.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashOpenSlip = model.CashOpenSlip.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }
            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashOpen(NewCashOpen cashOpen)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null,
                resultType = ResultType.Information
            };

            CashControlModel model = new CashControlModel();

            if (cashOpen != null)
            {


                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashOpen.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashOpen.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var amount = Convert.ToDouble(cashOpen.Amount.Replace(".", ","));
                var currency = cashOpen.Currency;
                var cash = OfficeHelper.GetCash(cashOpen.LocationID, cashOpen.Currency);
                var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                var isOpen = Db.DocumentCashOpen.FirstOrDefault(x => x.LocationID == cashOpen.LocationID && x.CashID == cash.ID && x.ActionTypeID == cashOpen.ActinTypeID);

                if (isOpen == null)
                {
                    var docDate = new DateTime(DateTime.Now.Year, 1, 1);

                    // kasa açılışı eklenir.

                    try
                    {


                        DocumentCashOpen newCashColl = new DocumentCashOpen();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.Amount = amount;
                        newCashColl.CashID = cash.ID;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = cashOpen.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "COS");
                        newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashOpen.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(3);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                        newCashColl.SystemCurrency = ourcompany.Currency;
                        newCashColl.EnvironmentID = 2;
                        newCashColl.UID = Guid.NewGuid();

                        Db.DocumentCashOpen.Add(newCashColl);
                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi başarı ile eklendi";

                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "CashOpen", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }
                else
                {
                    // Update edilir.
                    try
                    {
                        DocumentCashOpen self = new DocumentCashOpen()
                        {
                            ActionTypeID = isOpen.ActionTypeID,
                            ActionTypeName = isOpen.ActionTypeName,
                            Amount = isOpen.Amount,
                            CashID = isOpen.CashID,
                            Currency = isOpen.Currency,
                            Date = isOpen.Date,
                            Description = isOpen.Description,
                            DocumentNumber = isOpen.DocumentNumber,
                            ExchangeRate = isOpen.ExchangeRate,
                            ID = isOpen.ID,
                            IsActive = isOpen.IsActive,
                            LocationID = isOpen.LocationID,
                            OurCompanyID = isOpen.OurCompanyID,
                            RecordDate = isOpen.RecordDate,
                            RecordEmployeeID = isOpen.RecordEmployeeID,
                            RecordIP = isOpen.RecordIP,
                            ReferenceID = isOpen.ReferenceID,
                            SystemAmount = isOpen.SystemAmount,
                            SystemCurrency = isOpen.SystemCurrency,
                            UpdateDate = isOpen.UpdateDate,
                            UpdateEmployee = isOpen.UpdateEmployee,
                            UpdateIP = isOpen.UpdateIP,
                            EnvironmentID = isOpen.EnvironmentID
                        };



                        isOpen.Amount = amount;
                        isOpen.Description = cashOpen.Description;
                        isOpen.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                        isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                        isOpen.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        // cash action da update edilir.
                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.CashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isOpen.Date && x.DocumentNumber == isOpen.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isOpen.Amount;
                            cashaction.UpdateDate = isOpen.UpdateDate;
                            cashaction.UpdateEmployeeID = isOpen.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki kasa açılış fişi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashOpen>(self, isOpen, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "CashOpen", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Open", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashOpen(NewCashOpen cashOpen)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (cashOpen != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashOpen.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashOpen.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var amount = Convert.ToDouble(cashOpen.Amount.Replace(".", ","));
                var currency = cashOpen.Currency;
                var cash = OfficeHelper.GetCash(cashOpen.LocationID, cashOpen.Currency);
                var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                var isOpen = Db.DocumentCashOpen.FirstOrDefault(x => x.LocationID == cashOpen.LocationID && x.CashID == cash.ID && x.ActionTypeID == cashOpen.ActinTypeID);
                if (isOpen != null)
                {
                    try
                    {
                        DocumentCashOpen self = new DocumentCashOpen()
                        {
                            ActionTypeID = isOpen.ActionTypeID,
                            ActionTypeName = isOpen.ActionTypeName,
                            Amount = isOpen.Amount,
                            CashID = isOpen.CashID,
                            Currency = isOpen.Currency,
                            Date = isOpen.Date,
                            Description = isOpen.Description,
                            DocumentNumber = isOpen.DocumentNumber,
                            ExchangeRate = isOpen.ExchangeRate,
                            ID = isOpen.ID,
                            IsActive = isOpen.IsActive,
                            LocationID = isOpen.LocationID,
                            OurCompanyID = isOpen.OurCompanyID,
                            RecordDate = isOpen.RecordDate,
                            RecordEmployeeID = isOpen.RecordEmployeeID,
                            RecordIP = isOpen.RecordIP,
                            ReferenceID = isOpen.ReferenceID,
                            SystemAmount = isOpen.SystemAmount,
                            SystemCurrency = isOpen.SystemCurrency,
                            UpdateDate = isOpen.UpdateDate,
                            UpdateEmployee = isOpen.UpdateEmployee,
                            UpdateIP = isOpen.UpdateIP,
                            EnvironmentID = isOpen.EnvironmentID
                        };



                        isOpen.Amount = amount;
                        isOpen.Description = cashOpen.Description;
                        isOpen.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                        isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                        isOpen.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        // cash action da update edilir.
                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.CashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isOpen.Date && x.DocumentNumber == isOpen.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isOpen.Amount;
                            cashaction.UpdateDate = isOpen.UpdateDate;
                            cashaction.UpdateEmployeeID = isOpen.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki kasa açılış fişi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashOpen>(self, isOpen, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "CashOpen", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            //return RedirectToAction("CashDetail", new { id = cashCollect.UID });
            return RedirectToAction("Open", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashOpen(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {

                var isOpen = Db.DocumentCashOpen.FirstOrDefault(x => x.ID == id);
                if (isOpen != null)
                {
                    try
                    {
                        DocumentCashOpen self = new DocumentCashOpen()
                        {
                            ActionTypeID = isOpen.ActionTypeID,
                            ActionTypeName = isOpen.ActionTypeName,
                            Amount = isOpen.Amount,
                            CashID = isOpen.CashID,
                            Currency = isOpen.Currency,
                            Date = isOpen.Date,
                            Description = isOpen.Description,
                            DocumentNumber = isOpen.DocumentNumber,
                            ExchangeRate = isOpen.ExchangeRate,
                            ID = isOpen.ID,
                            IsActive = isOpen.IsActive,
                            LocationID = isOpen.LocationID,
                            OurCompanyID = isOpen.OurCompanyID,
                            RecordDate = isOpen.RecordDate,
                            RecordEmployeeID = isOpen.RecordEmployeeID,
                            RecordIP = isOpen.RecordIP,
                            ReferenceID = isOpen.ReferenceID,
                            SystemAmount = isOpen.SystemAmount,
                            SystemCurrency = isOpen.SystemCurrency,
                            UpdateDate = isOpen.UpdateDate,
                            UpdateEmployee = isOpen.UpdateEmployee,
                            UpdateIP = isOpen.UpdateIP,
                            EnvironmentID = isOpen.EnvironmentID
                        };



                        isOpen.IsActive = false;
                        isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                        //isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                        //isOpen.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();
                        OfficeHelper.AddCashAction(isOpen.CashID, isOpen.LocationID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, 1, -1 * isOpen.Amount, 0, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isOpen.ID} ID li {isOpen.Amount} {isOpen.Currency} tutarındaki kasa açılış fişi başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashOpen>(self, isOpen, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isOpen.ID.ToString(), "Cash", "CashOpen", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isOpen.Amount} {isOpen.Currency} tutarındaki kasa açılış fişi iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Open", "Cash");

        }

        [AllowAnonymous]
        public ActionResult OpenDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.OpenDetail = Db.VDocumentCashOpen.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "CashOpen" && x.Environment == "Office" && x.ProcessID == model.CashDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult CashPayment(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashPayments = Db.VDocumentCashPayments.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashPayments = model.CashPayments.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashPayment(NewCashPayments cashPayment)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashPayment != null)
            {

                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashPayment.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashPayment.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashPayment.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashPayment.FromID.Substring(1, cashPayment.FromID.Length - 1));
                var amount = Convert.ToDouble(cashPayment.Amount.Replace(".", ","));
                var currency = cashPayment.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashPayment.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashPayment.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashPayment.LocationID, cashPayment.Currency);
                // tahsilat eklenir.

                try
                {
                    var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                    if (balance >= amount)
                    {
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                        DocumentCashPayments newCashColl = new DocumentCashPayments();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.Amount = amount;
                        newCashColl.CashID = cash.ID;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = cashPayment.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "CPY");
                        newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        newCashColl.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        newCashColl.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashPayment.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                        newCashColl.SystemCurrency = ourcompany.Currency;
                        newCashColl.EnvironmentID = 2;
                        newCashColl.UID = Guid.NewGuid();

                        Db.DocumentCashPayments.Add(newCashColl);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Kasa Ödemesi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
                catch (Exception ex)
                {

                    result.Message = $"Kasa Ödemesi eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("CashPayment", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashPayment(NewCashCollect cashCollect)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);



                var isCash = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentCashPayments self = new DocumentCashPayments()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
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
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        isCash.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.CashID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Payment = isCash.Amount;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki kasa ödemesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashPayments>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "CashPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki kasa ödemesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            //return RedirectToAction("CashDetail", new { id = cashCollect.UID });
            return RedirectToAction("CashPayment", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashPayment(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {

                var isCash = Db.DocumentCashPayments.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentCashPayments self = new DocumentCashPayments()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            CashID = isCash.CashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
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
                            ToEmployeeID = isCash.ToEmployeeID,
                            ToCustomerID = isCash.ToCustomerID,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);


                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "CashPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki kasa ödemesi iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "CashPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("CashPayment", "Cash");

        }

        [AllowAnonymous]
        public ActionResult PaymentDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.PaymentDetail = Db.VDocumentCashPayments.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "CashPayment" && x.Environment == "Office" && x.ProcessID == model.PaymentDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult SaleReturn(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.TicketSalesReturn = Db.VDocumentTicketSaleReturn.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.TicketSalesReturn = model.TicketSalesReturn.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddTicketSaleReturn(NewTicketSaleReturn cashSaleReturn)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashSaleReturn != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSaleReturn.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSaleReturn.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSaleReturn.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSaleReturn.FromID.Substring(1, cashSaleReturn.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSaleReturn.Amount.Replace(".", ","));
                var currency = cashSaleReturn.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSaleReturn.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSaleReturn.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSaleReturn.LocationID, cashSaleReturn.Currency);



                try
                {
                    var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                    if (balance >= amount)
                    {
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                        DocumentTicketSaleReturns newCashColl = new DocumentTicketSaleReturns();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.Amount = amount;
                        newCashColl.CashID = cash.ID;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = cashSaleReturn.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "TSR");
                        newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        newCashColl.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashSaleReturn.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(3);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                        newCashColl.SystemCurrency = ourcompany.Currency;
                        newCashColl.PayMethodID = 1;
                        newCashColl.Quantity = cashSaleReturn.Quantity;
                        newCashColl.PayMethodID = cashSaleReturn.PayMethodID;
                        newCashColl.EnvironmentID = 2;
                        newCashColl.UID = Guid.NewGuid();

                        Db.DocumentTicketSaleReturns.Add(newCashColl);
                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Bilet satış iadesi başarı ile eklendi";

                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
                catch (Exception ex)
                {
                    result.Message = $"Bilet satış iadesi eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }
            }

            TempData["result"] = result;

            return RedirectToAction("SaleReturn", "Cash");
        }

        [AllowAnonymous]
        public ActionResult Expense(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashExpense = Db.VDocumentCashExpense.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashExpense = model.CashExpense.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashExpense(NewCashExpense cashExpense, HttpPostedFileBase documentFile)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashExpense != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashExpense.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashExpense.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashExpense.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashExpense.FromID.Substring(1, cashExpense.FromID.Length - 1));
                var amount = Convert.ToDouble(cashExpense.Amount.Replace(".", ","));
                var currency = cashExpense.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashExpense.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashExpense.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashExpense.LocationID, cashExpense.Currency);
                // tahsilat eklenir.

                try
                {
                    var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                    if (balance >= amount)
                    {
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                        DocumentCashExpense newCashColl = new DocumentCashExpense();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.Amount = amount;
                        newCashColl.CashID = cash.ID;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = cashExpense.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXP");
                        newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        newCashColl.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                        newCashColl.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        newCashColl.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashExpense.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                        newCashColl.SystemCurrency = ourcompany.Currency;
                        newCashColl.SlipNumber = cashExpense.SlipNumber;
                        newCashColl.EnvironmentID = 2;
                        newCashColl.UID = Guid.NewGuid();

                        string FileName = string.Empty;

                        if (documentFile != null)
                        {
                            FileName = Guid.NewGuid().ToString();
                            string ext = System.IO.Path.GetExtension(documentFile.FileName);
                            FileName = FileName + ext;

                            if (documentFile != null && documentFile.ContentLength > 0)
                            {
                                try
                                {
                                    documentFile.SaveAs(Path.Combine(Server.MapPath("../Document/Expense"), FileName));
                                    newCashColl.SlipDocument = FileName;
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }

                        Db.DocumentCashExpense.Add(newCashColl);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Masraf ödeme fişi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
                catch (Exception ex)
                {

                    result.Message = $"Masraf ödeme fişi eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("Expense", "Cash");
        }

        [AllowAnonymous]
        public ActionResult BankTransfer(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.BankTransfer = Db.VDocumentBankTransfer.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.BankTransfer = model.BankTransfer.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashBankTransfer(NewCashBankTransfer cashTransfer, HttpPostedFileBase documentFile)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashTransfer != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashTransfer.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashTransfer.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashTransfer.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashTransfer.FromID.Substring(1, cashTransfer.FromID.Length - 1));
                var amount = Convert.ToDouble(cashTransfer.Amount.Replace(".", ","));
                var currency = cashTransfer.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashTransfer.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashTransfer.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashTransfer.LocationID, cashTransfer.Currency);
                // tahsilat eklenir.
                var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                var isOpen = Db.DocumentBankTransfer.FirstOrDefault(x => x.ReferenceCode == cashTransfer.ReferenceCode && x.StatusID == 1);
                if (isOpen == null)
                {
                    try
                    {
                        var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                        if (balance >= amount)
                        {
                            DocumentBankTransfer newCashColl = new DocumentBankTransfer();

                            newCashColl.ActionTypeID = actType.ID;
                            newCashColl.ActionTypeName = actType.Name;
                            newCashColl.Amount = amount;
                            newCashColl.FromCashID = cash.ID;
                            newCashColl.Currency = currency;
                            newCashColl.Date = docDate;
                            newCashColl.Description = cashTransfer.Description;
                            newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "BT");
                            newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                            newCashColl.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                            newCashColl.IsActive = true;
                            newCashColl.LocationID = cashTransfer.LocationID;
                            newCashColl.OurCompanyID = location.OurCompanyID;
                            newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                            newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                            newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                            newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                            newCashColl.SystemCurrency = ourcompany.Currency;
                            newCashColl.SlipNumber = cashTransfer.SlipNumber;
                            newCashColl.StatusID = 1;
                            newCashColl.EnvironmentID = 2;

                            string FileName = string.Empty;

                            if (documentFile != null)
                            {
                                FileName = Guid.NewGuid().ToString();
                                string ext = System.IO.Path.GetExtension(documentFile.FileName);
                                FileName = FileName + ext;

                                if (documentFile != null && documentFile.ContentLength > 0)
                                {
                                    try
                                    {
                                        documentFile.SaveAs(Path.Combine(Server.MapPath("../Document/Bank"), FileName));
                                        newCashColl.SlipDocument = FileName;
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }
                            List<string> varmi = Db.DocumentBankTransfer.Select(x => (string)x.ReferenceCode).ToList();

                            string rndNumber = location.OurCompanyID.ToString() + DateTime.Now.ToString("yy");
                            Random rnd = new Random();
                            for (int i = 1; i < 6; i++)
                            {
                                rndNumber += rnd.Next(0, 9).ToString();
                            }
                            if (!varmi.Contains((string)rndNumber))
                            {
                                newCashColl.ReferenceCode = rndNumber;
                            }
                            else
                            {
                                for (int i = 1; i < 6; i++)
                                {
                                    rndNumber += rnd.Next(0, 9).ToString();
                                }
                                newCashColl.ReferenceCode = rndNumber;
                            }



                            Db.DocumentBankTransfer.Add(newCashColl);
                            Db.SaveChanges();

                            // cari hesap işlemesi
                            OfficeHelper.AddCashAction(newCashColl.FromCashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                            result.IsSuccess = true;
                            result.Message = "Havale / EFT başarı ile eklendi";

                            // log atılır
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }


                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                    if (balance >= amount)
                    {
                        if (cashTransfer.StatusID == 2)
                        {
                            DocumentBankTransfer self = new DocumentBankTransfer()
                            {
                                ActionTypeID = isOpen.ActionTypeID,
                                ActionTypeName = isOpen.ActionTypeName,
                                Amount = isOpen.Amount,
                                FromCashID = isOpen.FromCashID,
                                ToBankAccountID = isOpen.ToBankAccountID,
                                Currency = isOpen.Currency,
                                Date = isOpen.Date,
                                Description = isOpen.Description,
                                DocumentNumber = isOpen.DocumentNumber,
                                ExchangeRate = isOpen.ExchangeRate,
                                ID = isOpen.ID,
                                IsActive = isOpen.IsActive,
                                LocationID = isOpen.LocationID,
                                OurCompanyID = isOpen.OurCompanyID,
                                RecordDate = isOpen.RecordDate,
                                RecordEmployeeID = isOpen.RecordEmployeeID,
                                RecordIP = isOpen.RecordIP,
                                ReferenceID = isOpen.ReferenceID,
                                SystemAmount = isOpen.SystemAmount,
                                SystemCurrency = isOpen.SystemCurrency,
                                UpdateDate = isOpen.UpdateDate,
                                UpdateEmployee = isOpen.UpdateEmployee,
                                UpdateIP = isOpen.UpdateIP,
                                EnvironmentID = isOpen.EnvironmentID

                            };


                            isOpen.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                            isOpen.Amount = amount;
                            isOpen.Description = cashTransfer.Description;
                            isOpen.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                            isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                            isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                            isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                            isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                            isOpen.SystemCurrency = ourcompany.Currency;
                            isOpen.StatusID = cashTransfer.StatusID;

                            Db.SaveChanges();

                            var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.FromCashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isOpen.Date && x.DocumentNumber == isOpen.DocumentNumber);

                            if (cashaction != null)
                            {
                                cashaction.Collection = isOpen.Amount;
                                cashaction.UpdateDate = isOpen.UpdateDate;
                                cashaction.UpdateEmployeeID = isOpen.UpdateEmployee;

                                Db.SaveChanges();

                            }

                            result.IsSuccess = true;
                            result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki havale eft işlemi başarı ile güncellendi";


                            var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isOpen, OfficeHelper.getIgnorelist());
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }
                        else
                        {
                            result.Message = $"Havale Eft işlemini onaylamanız gerekmektedir.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }

                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }

                }


            }

            TempData["result"] = result;

            return RedirectToAction("BankTransfer", "Cash");
        }

        [AllowAnonymous]
        public ActionResult SalaryPayment(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
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
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.SalaryPayment = Db.VDocumentSalaryPayment.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.SalaryPayment = model.SalaryPayment.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetPersonList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashSalaryPayment(NewCashSalaryPayment cashSalary)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();

            if (cashSalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSalary.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSalary.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSalary.FromID.Substring(1, cashSalary.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", ","));
                var currency = cashSalary.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
                // tahsilat eklenir.

                try
                {
                    var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                    if (balance >= amount)
                    {
                        var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                        DocumentSalaryPayment newCashColl = new DocumentSalaryPayment();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        newCashColl.Amount = amount;
                        newCashColl.FromCashID = (int?)cashSalary.BankAccountID == 0 ? cash.ID : (int?)null;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = cashSalary.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "SAP");
                        newCashColl.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        newCashColl.FromBankAccountID = (int?)cashSalary.BankAccountID > 0 ? cashSalary.BankAccountID : (int?)null;
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashSalary.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                        newCashColl.SystemCurrency = ourcompany.Currency;
                        newCashColl.SalaryType = cashSalary.SalaryType;

                        Db.DocumentSalaryPayment.Add(newCashColl);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(newCashColl.FromCashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        //maaş hesap işlemi
                        OfficeHelper.AddEmployeeAction(newCashColl.ToEmployeeID, newCashColl.ActionTypeID, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, cashSalary.SalaryType, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
                catch (Exception ex)
                {

                    result.Message = $"Maaş Avans eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("SalaryPayment", "Cash");
        }

    }
}