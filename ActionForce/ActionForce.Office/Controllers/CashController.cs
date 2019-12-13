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
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
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

                CashCollection collection = new CashCollection();
                collection.ActinTypeID = actType.ID;
                collection.Amount = amount;
                collection.Currency = currency;
                collection.Description = cashCollect.Description;
                collection.DocumentDate = docDate;
                collection.EnvironmentID = 2;
                collection.FromBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                collection.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                collection.FromEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                collection.LocationID = cashCollect.LocationID;
                
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddCashCollection(collection, model.Authentication);
            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashCollection(NewCashCollect cashCollect)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (cashCollect != null)
            {
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                var exchanges = !string.IsNullOrEmpty(cashCollect.ExchangeRate) ? Convert.ToDouble(cashCollect.ExchangeRate.ToString().Replace(".", ",")) : (double?)null ;
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }

                CashCollection collection = new CashCollection();
                collection.ActinTypeID = cashCollect.ActinTypeID;
                collection.Amount = amount;
                collection.Currency = currency;
                collection.Description = cashCollect.Description;
                collection.DocumentDate = docDate;
                collection.EnvironmentID = 2;
                collection.FromBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                collection.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                collection.FromEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                collection.LocationID = cashCollect.LocationID;
                collection.UID = cashCollect.UID;
                collection.ExchangeRate = exchanges;

                DocumentManager documentManager = new DocumentManager();
                result = documentManager.EditCashCollection(collection, model.Authentication);
            }

            TempData["result"] = result;
            return RedirectToAction("CashDetail", new { id = cashCollect.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashCollection(long? id)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeleteCashCollection(id, model.Authentication);
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
                var refID = string.IsNullOrEmpty(cashSale.ReferanceID);
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
                    newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

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
                var exchanges = cashCollect.ExchangeRate;
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);

                var isDate = DateTime.Now.Date;
                var isKasa = cash.ID;

                var isCash = Db.DocumentTicketSales.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.CashID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

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
                        isCash.CashID = cash.ID;
                        isCash.Date = docDate;
                        isCash.Quantity = isCash.Quantity;
                        isCash.PayMethodID = isCash.PayMethodID;
                        isCash.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(cashCollect.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isCash.Amount;
                            cashaction.CashID = cash.ID;
                            cashaction.Currency = cashCollect.Currency;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
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
            return RedirectToAction("SaleDetail", new { id = cashCollect.UID });
            //return RedirectToAction("Sale", "Cash");

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

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

                var refID = string.IsNullOrEmpty(cashSale.ReferanceID);

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
                        newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

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
                var exchanges = cashCollect.ExchangeRate;
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }

                var casho = OfficeHelper.GetCash(cashCollect.LocationID, currencyo);
                var cashi = OfficeHelper.GetCash(cashCollect.LocationID, currencyi);

                var isDate = DateTime.Now.Date;
                var isKasa = cashi.ID;
                var isKasao = casho.ID;

                var isCash = Db.DocumentSaleExchange.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.ToCashID);
                        isKasao = Convert.ToInt32(isCash.FromCashID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

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
                        isCash.LocationID = cashCollect.LocationID;
                        isCash.FromCashID = casho.ID;
                        isCash.ToCashID = cashi.ID;
                        isCash.Date = docDate;
                        isCash.Amount = amount;
                        isCash.ToCurrency = currencyi;
                        isCash.Currency = currencyo;
                        isCash.ToAmount = (isCash.Amount * isCash.SaleExchangeRate);
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(cashCollect.ExchangeRate.ToString().Replace(".", ",")) : currencyo == "USD" ? exchange.USDA : currencyo == "EUR" ? exchange.EURA : 1;
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
                        

                        Db.SaveChanges();
                        
                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasao && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);
                        var cashactioni = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);
                        if (cashaction != null)
                        {
                            cashaction.CashID = casho.ID;
                            cashaction.Collection = isCash.Amount;
                            cashaction.Currency = cashCollect.Currency;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }
                        if (cashactioni != null)
                        {
                            cashaction.CashID = cashi.ID;
                            cashactioni.Collection = isCash.ToAmount;
                            cashactioni.Currency = isCash.ToCurrency;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
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
            return RedirectToAction("ExchangeDetail", new { id = cashCollect.UID });
            //return RedirectToAction("Exchange", "Cash");

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

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

                var refID = string.IsNullOrEmpty(cashOpen.ReferanceID);

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
                        newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

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
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var exchanges = cashOpen.ExchangeRate;
                
                var isKasa = cash.ID;

                var isOpen = Db.DocumentCashOpen.FirstOrDefault(x => x.LocationID == cashOpen.LocationID && x.CashID == cash.ID && x.ActionTypeID == cashOpen.ActinTypeID);
                if (isOpen != null)
                {
                    try
                    {
                        isKasa = Convert.ToInt32(isOpen.CashID);

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
                        isOpen.ExchangeRate = exchanges != null ? Convert.ToDouble(cashOpen.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                        isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                        isOpen.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        // cash action da update edilir.
                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isOpen.Date && x.DocumentNumber == isOpen.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Collection = isOpen.Amount;
                            cashaction.Currency = cashOpen.Currency;
                            cashaction.CashID = cash.ID;
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
            return RedirectToAction("OpenDetail", new { id = cashOpen.UID });
            //return RedirectToAction("Open", "Cash");

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A" || x.Prefix == "E").ToList();

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

                var refID = string.IsNullOrEmpty(cashPayment.ReferanceID);

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
                        newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

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
        public ActionResult EditCashPayment(NewCashPayments cashCollect)
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
                var exchanges = cashCollect.ExchangeRate;
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);

                var isDate = DateTime.Now.Date;
                var isKasa = cash.ID;

                var isCash = Db.DocumentCashPayments.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.CashID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

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

                        isCash.Date = docDate;
                        isCash.CashID = cash.ID;
                        isCash.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        isCash.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(cashCollect.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Payment = isCash.Amount;
                            cashaction.Currency = cashCollect.Currency;
                            cashaction.CashID = cash.ID;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
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
            return RedirectToAction("PaymentDetail", new { id = cashCollect.UID });
            //return RedirectToAction("CashPayment", "Cash");

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A" || x.Prefix == "E").ToList();

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

                var refID = string.IsNullOrEmpty(cashSaleReturn.ReferanceID);

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
                        newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditTicketSaleReturn(NewTicketSaleReturn cashCollect)
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
                var exchanges = cashCollect.ExchangeRate;
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);

                var isDate = DateTime.Now.Date;
                var isKasa = cash.ID;

                var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.CashID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentTicketSaleReturns self = new DocumentTicketSaleReturns()
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
                        isCash.CashID = cash.ID;
                        isCash.Date = docDate;
                        isCash.Quantity = isCash.Quantity;
                        isCash.PayMethodID = isCash.PayMethodID;
                        isCash.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(cashCollect.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.CashID = cash.ID;
                            cashaction.Payment = isCash.Amount;
                            cashaction.Currency = cashCollect.Currency;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSaleReturns>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "SaleReturn", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("SaleRefundDetail", new { id = cashCollect.UID });
            //return RedirectToAction("SaleReturn", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteTicketSaleReturn(int? id)
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

                var isCash = Db.DocumentTicketSaleReturns.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentTicketSaleReturns self = new DocumentTicketSaleReturns()
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
                            ToCustomerID = isCash.ToCustomerID,
                            PayMethodID = isCash.PayMethodID,
                            Quantity = isCash.Quantity,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iadesi başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentTicketSales>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "SaleReturn", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki {isCash.Quantity} adet bilet satış iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "SaleReturn", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("SaleReturn", "Cash");

        }

        [AllowAnonymous]
        public ActionResult SaleRefundDetail(Guid? id)
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

            model.SalesRefundDetail = Db.VDocumentTicketSaleReturn.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "SaleReturn" && x.Environment == "Office" && x.ProcessID == model.SalesRefundDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
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
                var refID = string.IsNullOrEmpty(cashExpense.ReferanceID);
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
                        newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashExpense(NewCashExpense cashExpense, HttpPostedFileBase documentFile)
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
                var amount = Convert.ToDouble(cashExpense.Amount.ToString().Replace(".", ","));
                var currency = cashExpense.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var exchanges = cashExpense.ExchangeRate;
                if (DateTime.TryParse(cashExpense.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashExpense.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashExpense.LocationID, cashExpense.Currency);

                var isDate = DateTime.Now.Date;
                var isKasa = cash.ID;

                var isCash = Db.DocumentCashExpense.FirstOrDefault(x => x.UID == cashExpense.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = Convert.ToInt32(isCash.CashID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentCashExpense self = new DocumentCashExpense()
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
                            SlipNumber = isCash.SlipNumber,
                            SlipDocument = isCash.SlipDocument,
                            EnvironmentID = isCash.EnvironmentID
                        };

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
                                    isCash.SlipDocument = FileName;
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        isCash.CashID = cash.ID;
                        isCash.Date = docDate;
                        isCash.SlipNumber = isCash.SlipNumber;
                        isCash.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                        isCash.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        isCash.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.Description = isCash.Description;
                        isCash.ExchangeRate = exchanges != null ? Convert.ToDouble(cashExpense.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Payment = isCash.Amount;
                            cashaction.Currency = cashExpense.Currency;
                            cashaction.CashID = cash.ID;
                            cashaction.ActionDate = docDate;
                            cashaction.ProcessDate = docDate;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {amount} {currency} tutarındaki masraf ödeme fişi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashExpense>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "Expense", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{amount} {currency} tutarındaki masraf ödeme fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("ExpenseDetail", new { id = cashExpense.UID });
            //return RedirectToAction("Expense", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashExpense(int? id)
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

                var isCash = Db.DocumentCashExpense.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentCashExpense self = new DocumentCashExpense()
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
                            ToBankAccountID = isCash.ToBankAccountID,
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
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.Amount} {isCash.Currency} tutarındaki masraf ödeme fişi iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "Expense", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Expense", "Cash");

        }

        [AllowAnonymous]
        public ActionResult ExpenseDetail(Guid? id)
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

            model.ExpenseDetail = Db.VDocumentCashExpense.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "Expense" && x.Environment == "Office" && x.ProcessID == model.ExpenseDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).ToList();

            return View(model);
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


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "B").ToList();

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
                var commision = Convert.ToDouble(cashTransfer.Commission.Replace(".", ","));
                var currency = cashTransfer.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                var refID = string.IsNullOrEmpty(cashTransfer.ReferanceID);

                DateTime sDate = Convert.ToDateTime(cashTransfer.SlipDate).Date;
                DateTime sDatetime = sDate.Add(Convert.ToDateTime(cashTransfer.SlipTime).TimeOfDay);

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
                            newCashColl.Commission = commision;
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
                            newCashColl.SlipDate = sDatetime;
                            newCashColl.StatusID = 1;
                            newCashColl.EnvironmentID = 2;
                            newCashColl.TrackingNumber = cashTransfer.TrackingNumber;
                            newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;
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


            }

            TempData["result"] = result;

            return RedirectToAction("BankTransfer", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashBankTransfer(NewCashBankTransfer cashTransfer, HttpPostedFileBase documentFile)
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
                var refID = string.IsNullOrEmpty(cashTransfer.ReferanceID);

                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashTransfer.ActinTypeID);
                var actxType = Db.CashActionType.FirstOrDefault(x => x.ID == 29);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashTransfer.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashTransfer.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashTransfer.FromID.Substring(1, cashTransfer.FromID.Length - 1));
                var amount = Convert.ToDouble(cashTransfer.Amount.ToString().Replace(".", ","));
                var commision = Convert.ToDouble(cashTransfer.Commission.Replace(".", ","));
                var currency = cashTransfer.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var exchanges = cashTransfer.ExchangeRate;
                var _statusID = cashTransfer.StatusID;
                if (DateTime.TryParse(cashTransfer.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashTransfer.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashTransfer.LocationID, cashTransfer.Currency);

                var isDate = DateTime.Now.Date;
                var isKasa = cash.ID;

                DateTime sDate = Convert.ToDateTime(cashTransfer.SlipDate).Date;
                DateTime sDatetime = sDate.Add(Convert.ToDateTime(cashTransfer.SlipTime).TimeOfDay);

                var isOpen = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == cashTransfer.UID);
                if (isOpen != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isOpen.Date);
                        isKasa = Convert.ToInt32(isOpen.FromCashID);
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        var balance = Db.GetCashBalance(location.LocationID, cash.ID).FirstOrDefault().Value;
                        if (balance >= amount)
                        {
                            DocumentBankTransfer self = new DocumentBankTransfer()
                            {
                                ActionTypeID = isOpen.ActionTypeID,
                                ActionTypeName = isOpen.ActionTypeName,
                                Amount = isOpen.Amount,
                                NetAmount = isOpen.NetAmount,
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
                                SlipNumber = isOpen.SlipNumber,
                                SlipDocument = isOpen.SlipDocument,
                                SlipDate = isOpen.SlipDate,
                                StatusID = isOpen.StatusID,
                                TrackingNumber = isOpen.TrackingNumber,
                                Commission = isOpen.Commission,
                                EnvironmentID = isOpen.EnvironmentID

                            };

                            string FileName = string.Empty;
                            if (cashTransfer.StatusID == 2)
                            {
                                

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
                                            isOpen.SlipDocument = FileName;
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                }
                                isOpen.Commission = commision;
                                isOpen.Date = docDate;
                                isOpen.FromCashID = cash.ID;
                                isOpen.SlipDate = sDatetime;
                                isOpen.SlipNumber = isOpen.SlipNumber;
                                isOpen.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                                isOpen.Amount = amount;
                                isOpen.Description = cashTransfer.Description;
                                isOpen.ExchangeRate = exchanges != null ? Convert.ToDouble(cashTransfer.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                                isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                                isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                                isOpen.SystemCurrency = ourcompany.Currency;
                                isOpen.StatusID = cashTransfer.StatusID;
                                isOpen.TrackingNumber = cashTransfer.TrackingNumber;

                                Db.SaveChanges();

                                
                                


                                var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.FromCashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isDate && x.DocumentNumber == isOpen.DocumentNumber);
                                if (cashaction == null)
                                {
                                    OfficeHelper.AddCashAction(isOpen.FromCashID, isOpen.LocationID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, -1, 0, isOpen.NetAmount, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                }

                                result.IsSuccess = true;
                                result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki havale eft işlemi başarı ile lokasyon takip etti";

                                var refAny = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isOpen.ID && x.Date == docDate && x.LocationID == isOpen.LocationID);
                                if (refAny == null)
                                {
                                    DocumentCashExpense newCashExp = new DocumentCashExpense();

                                    newCashExp.ActionTypeID = actxType.ID;
                                    newCashExp.ActionTypeName = actxType.Name;
                                    newCashExp.Amount = commision;
                                    newCashExp.CashID = cash.ID;
                                    newCashExp.Currency = currency;
                                    newCashExp.Date = docDate;
                                    newCashExp.Description = cashTransfer.Description;
                                    newCashExp.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXP");
                                    newCashExp.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                                    newCashExp.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                                    newCashExp.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                                    newCashExp.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                                    newCashExp.IsActive = true;
                                    newCashExp.LocationID = cashTransfer.LocationID;
                                    newCashExp.OurCompanyID = location.OurCompanyID;
                                    newCashExp.RecordDate = DateTime.UtcNow.AddHours(timezone);
                                    newCashExp.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                                    newCashExp.RecordIP = OfficeHelper.GetIPAddress();
                                    newCashExp.SystemAmount = ourcompany.Currency == currency ? commision : commision * newCashExp.ExchangeRate;
                                    newCashExp.SystemCurrency = ourcompany.Currency;
                                    newCashExp.SlipNumber = cashTransfer.SlipNumber;
                                    newCashExp.EnvironmentID = 2;
                                    newCashExp.UID = Guid.NewGuid();
                                    newCashExp.ReferenceID = isOpen.ID;

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
                                                newCashExp.SlipDocument = FileName;
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }

                                    Db.DocumentCashExpense.Add(newCashExp);
                                    Db.SaveChanges();

                                    var expaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isOpen.LocationID && x.CashActionTypeID == actxType.ID && x.ProcessID == newCashExp.ID && x.ProcessDate == isDate && x.DocumentNumber == newCashExp.DocumentNumber);
                                    if (expaction == null)
                                    {

                                        OfficeHelper.AddCashAction(newCashExp.CashID, newCashExp.LocationID, null, newCashExp.ActionTypeID, newCashExp.Date, newCashExp.ActionTypeName, newCashExp.ID, newCashExp.Date, newCashExp.DocumentNumber, newCashExp.Description, -1, 0, newCashExp.Amount, newCashExp.Currency, null, null, newCashExp.RecordEmployeeID, newCashExp.RecordDate);
                                    }
                                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashExp.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                                }
                                

                                var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isOpen, OfficeHelper.getIgnorelist());
                                OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }
                            else if (cashTransfer.StatusID == 3)
                            {


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
                                            isOpen.SlipDocument = FileName;
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                }
                                isOpen.Commission = commision;
                                isOpen.Date = docDate;
                                isOpen.FromCashID = cash.ID;
                                isOpen.SlipDate = sDatetime;
                                isOpen.SlipNumber = isOpen.SlipNumber;
                                isOpen.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                                isOpen.Amount = amount;
                                isOpen.Description = cashTransfer.Description;
                                isOpen.ExchangeRate = exchanges != null ? Convert.ToDouble(cashTransfer.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                                isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                                isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                                isOpen.SystemCurrency = ourcompany.Currency;
                                isOpen.StatusID = cashTransfer.StatusID;
                                isOpen.TrackingNumber = cashTransfer.TrackingNumber;

                                Db.SaveChanges();

                                var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.FromCashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isDate && x.DocumentNumber == isOpen.DocumentNumber);
                                if (cashaction == null)
                                {
                                    OfficeHelper.AddCashAction(isOpen.FromCashID, isOpen.LocationID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, -1, 0, isOpen.NetAmount, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                }

                                result.IsSuccess = true;
                                result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki havale eft işlemi başarı ile lokasyon onaylandı";

                                var refAny = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isOpen.ID && x.Date == docDate && x.LocationID == isOpen.LocationID);
                                if (refAny == null)
                                {
                                    DocumentCashExpense newCashExp = new DocumentCashExpense();

                                    newCashExp.ActionTypeID = actxType.ID;
                                    newCashExp.ActionTypeName = actxType.Name;
                                    newCashExp.Amount = commision;
                                    newCashExp.CashID = cash.ID;
                                    newCashExp.Currency = currency;
                                    newCashExp.Date = docDate;
                                    newCashExp.Description = cashTransfer.Description;
                                    newCashExp.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXP");
                                    newCashExp.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                                    newCashExp.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                                    newCashExp.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                                    newCashExp.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                                    newCashExp.IsActive = true;
                                    newCashExp.LocationID = cashTransfer.LocationID;
                                    newCashExp.OurCompanyID = location.OurCompanyID;
                                    newCashExp.RecordDate = DateTime.UtcNow.AddHours(timezone);
                                    newCashExp.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                                    newCashExp.RecordIP = OfficeHelper.GetIPAddress();
                                    newCashExp.SystemAmount = ourcompany.Currency == currency ? commision : commision * newCashExp.ExchangeRate;
                                    newCashExp.SystemCurrency = ourcompany.Currency;
                                    newCashExp.SlipNumber = cashTransfer.SlipNumber;
                                    newCashExp.EnvironmentID = 2;
                                    newCashExp.UID = Guid.NewGuid();
                                    newCashExp.ReferenceID = isOpen.ID;

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
                                                newCashExp.SlipDocument = FileName;
                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                    }

                                    Db.DocumentCashExpense.Add(newCashExp);
                                    Db.SaveChanges();

                                    var expaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isOpen.LocationID && x.CashActionTypeID == actxType.ID && x.ProcessID == newCashExp.ID && x.ProcessDate == isDate && x.DocumentNumber == newCashExp.DocumentNumber);
                                    if (expaction == null)
                                    {

                                        OfficeHelper.AddCashAction(newCashExp.CashID, newCashExp.LocationID, null, newCashExp.ActionTypeID, newCashExp.Date, newCashExp.ActionTypeName, newCashExp.ID, newCashExp.Date, newCashExp.DocumentNumber, newCashExp.Description, -1, 0, newCashExp.Amount, newCashExp.Currency, null, null, newCashExp.RecordEmployeeID, newCashExp.RecordDate);
                                    }
                                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashExp.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                                }

                                


                                var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isOpen, OfficeHelper.getIgnorelist());
                                OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }
                            else if (cashTransfer.StatusID == 4)
                            {
                                isOpen.Commission = commision;
                                isOpen.Date = docDate;
                                isOpen.FromCashID = cash.ID;
                                isOpen.SlipNumber = isOpen.SlipNumber;
                                isOpen.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                                isOpen.Amount = amount;
                                isOpen.Description = cashTransfer.Description;
                                isOpen.ExchangeRate = exchanges != null ? Convert.ToDouble(cashTransfer.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                                isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                                isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                                isOpen.SystemCurrency = ourcompany.Currency;
                                isOpen.StatusID = cashTransfer.StatusID;
                                isOpen.TrackingNumber = cashTransfer.TrackingNumber;

                                Db.SaveChanges();

                                
                                result.IsSuccess = true;
                                result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki havale eft işlemi başarı ile muhasebe takip etti";


                                var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isOpen, OfficeHelper.getIgnorelist());
                                OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }
                            else if (cashTransfer.StatusID == 5)
                            {
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
                                            isOpen.SlipDocument = FileName;
                                        }
                                        catch (Exception ex)
                                        {
                                        }
                                    }
                                }
                                isOpen.Commission = commision;
                                isOpen.Date = docDate;
                                isOpen.FromCashID = cash.ID;
                                isOpen.SlipNumber = isOpen.SlipNumber;
                                isOpen.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                                isOpen.Amount = amount;
                                isOpen.Description = cashTransfer.Description;
                                isOpen.ExchangeRate = exchanges != null ? Convert.ToDouble(cashTransfer.ExchangeRate.ToString().Replace(".", ",")) : currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                                isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                isOpen.UpdateIP = OfficeHelper.GetIPAddress();
                                isOpen.SystemAmount = ourcompany.Currency == currency ? amount : amount * isOpen.ExchangeRate;
                                isOpen.SystemCurrency = ourcompany.Currency;
                                isOpen.StatusID = cashTransfer.StatusID;
                                isOpen.TrackingNumber = cashTransfer.TrackingNumber;

                                Db.SaveChanges();

                                OfficeHelper.AddBankAction(isOpen.LocationID, null, isOpen.ToBankAccountID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, 1, isOpen.NetAmount, 0, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                

                                result.IsSuccess = true;
                                result.Message = $"{isOpen.ID} ID li {amount} {currency} tutarındaki havale eft işlemi başarı ile muhasebe onayladı";


                                var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentBankTransfer>(self, isOpen, OfficeHelper.getIgnorelist());
                                OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "BankTransfer", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }
                            else if (cashTransfer.StatusID == 6)
                            {
                                isOpen.IsActive = false;
                                isOpen.StatusID = 6;
                                isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                isOpen.UpdateIP = OfficeHelper.GetIPAddress();

                                Db.SaveChanges();

                                OfficeHelper.AddCashAction(isOpen.FromCashID, isOpen.LocationID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, -1, 0, -1 * isOpen.NetAmount, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                OfficeHelper.AddBankAction(isOpen.LocationID, null, isOpen.ToBankAccountID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, 1, -1 * isOpen.NetAmount, 0, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                result.IsSuccess = true;
                                result.Message = $"{isOpen.ID} ID li {isOpen.Amount} {isOpen.Currency} tutarındaki havale eft işlemi muhasebe red etti";

                                var isExp = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isOpen.ID && x.Date == docDate && x.LocationID == isOpen.LocationID);
                                if (isExp != null)
                                {
                                    DocumentCashExpense selfs = new DocumentCashExpense()
                                    {
                                        ActionTypeID = isExp.ActionTypeID,
                                        ActionTypeName = isExp.ActionTypeName,
                                        Amount = isExp.Amount,
                                        CashID = isExp.CashID,
                                        Currency = isExp.Currency,
                                        Date = isExp.Date,
                                        Description = isExp.Description,
                                        DocumentNumber = isExp.DocumentNumber,
                                        ExchangeRate = isExp.ExchangeRate,
                                        ID = isExp.ID,
                                        IsActive = isExp.IsActive,
                                        LocationID = isExp.LocationID,
                                        OurCompanyID = isExp.OurCompanyID,
                                        RecordDate = isExp.RecordDate,
                                        RecordEmployeeID = isExp.RecordEmployeeID,
                                        RecordIP = isExp.RecordIP,
                                        ReferenceID = isExp.ReferenceID,
                                        SystemAmount = isExp.SystemAmount,
                                        SystemCurrency = isExp.SystemCurrency,
                                        UpdateDate = isExp.UpdateDate,
                                        UpdateEmployee = isExp.UpdateEmployee,
                                        UpdateIP = isExp.UpdateIP,
                                        ToBankAccountID = isExp.ToBankAccountID,
                                        ToEmployeeID = isExp.ToEmployeeID,
                                        ToCustomerID = isExp.ToCustomerID,
                                        EnvironmentID = isExp.EnvironmentID
                                    };


                                    isExp.IsActive = false;
                                    isExp.UpdateDate = DateTime.UtcNow.AddHours(3);
                                    isExp.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                    isExp.UpdateIP = OfficeHelper.GetIPAddress();

                                    Db.SaveChanges();

                                    var expaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isOpen.LocationID && x.CashActionTypeID == actxType.ID && x.ProcessID == isExp.ID && x.ProcessDate == isDate && x.DocumentNumber == isExp.DocumentNumber);
                                    if (expaction != null)
                                    {

                                        OfficeHelper.AddCashAction(isExp.CashID, isExp.LocationID, null, isExp.ActionTypeID, isExp.Date, isExp.ActionTypeName, isExp.ID, isExp.Date, isExp.DocumentNumber, isExp.Description, -1, 0, -1 * isExp.Amount, isExp.Currency, null, null, isExp.RecordEmployeeID, isExp.RecordDate);
                                    }

                                    OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isExp.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                                }

                                OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isOpen.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }
                            else if (cashTransfer.StatusID == 7)
                            {
                                isOpen.IsActive = false;
                                isOpen.StatusID = 7;
                                isOpen.UpdateDate = DateTime.UtcNow.AddHours(3);
                                isOpen.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                isOpen.UpdateIP = OfficeHelper.GetIPAddress();

                                Db.SaveChanges();

                                var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isOpen.FromCashID && x.LocationID == isOpen.LocationID && x.CashActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isDate && x.DocumentNumber == isOpen.DocumentNumber);
                                if (cashaction != null)
                                {
                                    OfficeHelper.AddCashAction(isOpen.FromCashID, isOpen.LocationID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, -1, 0, -1 * isOpen.NetAmount, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                }
                                var bankaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isKasa && x.LocationID == isOpen.LocationID && x.BankActionTypeID == isOpen.ActionTypeID && x.ProcessID == isOpen.ID && x.ProcessDate == isDate && x.DocumentNumber == isOpen.DocumentNumber);
                                if (bankaction != null)
                                {
                                    OfficeHelper.AddBankAction(isOpen.LocationID, null, isOpen.ToBankAccountID, null, isOpen.ActionTypeID, isOpen.Date, isOpen.ActionTypeName, isOpen.ID, isOpen.Date, isOpen.DocumentNumber, isOpen.Description, 1, -1 * isOpen.NetAmount, 0, isOpen.Currency, null, null, isOpen.RecordEmployeeID, isOpen.RecordDate);
                                }
                                result.IsSuccess = true;
                                result.Message = $"{isOpen.ID} ID li {isOpen.Amount} {isOpen.Currency} tutarındaki havale eft işlemi iptal edildi";

                                var isExp = Db.DocumentCashExpense.FirstOrDefault(x => x.ReferenceID == isOpen.ID && x.Date == docDate && x.LocationID == isOpen.LocationID);
                                if (isExp != null)
                                {
                                    DocumentCashExpense selfs = new DocumentCashExpense()
                                    {
                                        ActionTypeID = isExp.ActionTypeID,
                                        ActionTypeName = isExp.ActionTypeName,
                                        Amount = isExp.Amount,
                                        CashID = isExp.CashID,
                                        Currency = isExp.Currency,
                                        Date = isExp.Date,
                                        Description = isExp.Description,
                                        DocumentNumber = isExp.DocumentNumber,
                                        ExchangeRate = isExp.ExchangeRate,
                                        ID = isExp.ID,
                                        IsActive = isExp.IsActive,
                                        LocationID = isExp.LocationID,
                                        OurCompanyID = isExp.OurCompanyID,
                                        RecordDate = isExp.RecordDate,
                                        RecordEmployeeID = isExp.RecordEmployeeID,
                                        RecordIP = isExp.RecordIP,
                                        ReferenceID = isExp.ReferenceID,
                                        SystemAmount = isExp.SystemAmount,
                                        SystemCurrency = isExp.SystemCurrency,
                                        UpdateDate = isExp.UpdateDate,
                                        UpdateEmployee = isExp.UpdateEmployee,
                                        UpdateIP = isExp.UpdateIP,
                                        ToBankAccountID = isExp.ToBankAccountID,
                                        ToEmployeeID = isExp.ToEmployeeID,
                                        ToCustomerID = isExp.ToCustomerID,
                                        EnvironmentID = isExp.EnvironmentID
                                    };


                                    isExp.IsActive = false;
                                    isExp.UpdateDate = DateTime.UtcNow.AddHours(3);
                                    isExp.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                                    isExp.UpdateIP = OfficeHelper.GetIPAddress();

                                    Db.SaveChanges();

                                    var expaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isOpen.LocationID && x.CashActionTypeID == actxType.ID && x.ProcessID == isExp.ID && x.ProcessDate == isDate && x.DocumentNumber == isExp.DocumentNumber);
                                    if (expaction != null)
                                    {

                                        OfficeHelper.AddCashAction(isExp.CashID, isExp.LocationID, null, isExp.ActionTypeID, isExp.Date, isExp.ActionTypeName, isExp.ID, isExp.Date, isExp.DocumentNumber, isExp.Description, -1, 0, -1 * isExp.Amount, isExp.Currency, null, null, isExp.RecordEmployeeID, isExp.RecordDate);
                                    }

                                    OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isExp.ID.ToString(), "Cash", "Expense", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                                }

                                OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isOpen.ID.ToString(), "Cash", "BankTransfer", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                            }

                        }
                        else
                        {
                            result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                            OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                        }

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Havale / EFT güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "BankTransfer", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("TransferDetail", new { id = cashTransfer.UID });
            //return RedirectToAction("BankTransfer", "Cash");

        }

        [AllowAnonymous]
        public ActionResult TransferDetail(Guid? id)
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

            model.TransferDetail = Db.VDocumentBankTransfer.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "BankTransfer" && x.Environment == "Office" && x.ProcessID == model.TransferDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "B").ToList();

            return View(model);
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


            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

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

                var refID = string.IsNullOrEmpty(cashSalary.ReferanceID);

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
                        newCashColl.ReferenceID = refID == true ? Convert.ToInt64(refID) : (long?)null;

                        Db.DocumentSalaryPayment.Add(newCashColl);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        if (cashSalary.BankAccountID > 0)
                        {
                            OfficeHelper.AddBankAction(newCashColl.LocationID, newCashColl.ToEmployeeID, newCashColl.FromBankAccountID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);
                        }
                        else
                        {
                            OfficeHelper.AddCashAction(newCashColl.FromCashID, newCashColl.LocationID, newCashColl.ToEmployeeID, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);
                        }
                        
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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashSalaryPayment(NewCashSalaryPayment cashCollect)
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
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);

                var isDate = DateTime.Now.Date;
                int? isKasa = cashCollect.BankAccountID;
                int? isBank = cashCollect.BankAccountID;
                var isEmp = fromPrefix == "E" ? fromID : (int?)null;

                var isCash = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isKasa = isCash.FromCashID != null ? Convert.ToInt32(isCash.FromCashID) : (int?)null;
                        isBank = isCash.FromBankAccountID != null ? isCash.FromBankAccountID : (int?)null;
                        isEmp = isCash.ToEmployeeID;
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentSalaryPayment self = new DocumentSalaryPayment()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            ToEmployeeID = isCash.ToEmployeeID,
                            FromCashID = isCash.FromCashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            FromBankAccountID = isCash.FromBankAccountID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            SalaryType = isCash.SalaryType,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            EnvironmentID = isCash.EnvironmentID
                        };

                        isCash.Date = docDate;
                        isCash.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                        isCash.Amount = amount;
                        isCash.FromCashID = (int?)cashCollect.BankAccountID == 0 ? cash.ID : (int?)null;
                        isCash.Description = cashCollect.Description;
                        isCash.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                        isCash.FromBankAccountID = (int?)cashCollect.BankAccountID > 0 ? cashCollect.BankAccountID : (int?)null;
                        isCash.SalaryType = cashCollect.SalaryType;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();
                        isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        if (isKasa != null && cashCollect.BankAccountID == 0)
                        {
                            var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.EmployeeID == isEmp && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                            if (cashaction != null)
                            {
                                cashaction.Payment = isCash.Amount;
                                cashaction.Currency = cashCollect.Currency;
                                cashaction.CashID = (int?)cashCollect.BankAccountID == 0 ? cash.ID : (int?)null;
                                cashaction.ActionDate = docDate;
                                cashaction.ProcessDate = docDate;
                                cashaction.EmployeeID = isCash.ToEmployeeID;
                                cashaction.UpdateDate = isCash.UpdateDate;
                                cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                                Db.SaveChanges();

                            }
                        }
                        else if (isBank != null && cashCollect.BankAccountID > 0)
                        {
                            var bankaction = Db.BankActions.FirstOrDefault(x => x.BankAccountID == isBank && x.LocationID == isCash.LocationID && x.EmployeeID == isEmp && x.BankActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                            if (bankaction != null)
                            {
                                bankaction.Payment = isCash.Amount;
                                bankaction.Currency = cashCollect.Currency;
                                bankaction.BankAccountID = (int?)cashCollect.BankAccountID > 0 ? cashCollect.BankAccountID : (int?)null;
                                bankaction.ActionDate = docDate;
                                bankaction.ProcessDate = docDate;
                                bankaction.EmployeeID = isCash.ToEmployeeID;
                                bankaction.UpdateDate = isCash.UpdateDate;
                                bankaction.UpdateEmployeeID = isCash.UpdateEmployee;

                                Db.SaveChanges();

                            }
                        }
                        else if (isKasa != null && cashCollect.BankAccountID != 0)
                        {
                            OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);
                            OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        }
                        else if (isBank != null && cashCollect.BankAccountID == 0)
                        {
                            OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0,  -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);
                            OfficeHelper.AddBankAction(isCash.LocationID, null, isCash.FromBankAccountID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);
                        }
                        

                        var empaction = Db.EmployeeCashActions.FirstOrDefault(x => x.EmployeeID == isEmp && x.ActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate);

                        if (empaction != null)
                        {
                            empaction.ProcessDate = docDate;
                            empaction.Collection = isCash.Amount;
                            empaction.Currency = cashCollect.Currency;
                            empaction.EmployeeID = isCash.ToEmployeeID;
                            empaction.UpdateDate = isCash.UpdateDate;
                            empaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi başarı ile güncellendi";

                        
                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSalaryPayment>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isCash.ID.ToString(), "Cash", "SalaryPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Maaş Avans güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Update", "-1", "Cash", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("SalaryDetail", new { id = cashCollect.UID });
            //return RedirectToAction("SalaryPayment", "Cash");

        }

        [AllowAnonymous]
        public ActionResult DeleteCashSalaryPayment(int? id)
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

                var isCash = Db.DocumentSalaryPayment.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentSalaryPayment self = new DocumentSalaryPayment()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            Amount = isCash.Amount,
                            ToEmployeeID = isCash.ToEmployeeID,
                            FromCashID = isCash.FromCashID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            ExchangeRate = isCash.ExchangeRate,
                            ID = isCash.ID,
                            FromBankAccountID = isCash.FromBankAccountID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            ReferenceID = isCash.ReferenceID,
                            SystemAmount = isCash.SystemAmount,
                            SystemCurrency = isCash.SystemCurrency,
                            SalaryType = isCash.SalaryType,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        //maaş hesap işlemi
                        OfficeHelper.AddEmployeeAction(isCash.ToEmployeeID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.SalaryType, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi iptal edildi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", isCash.ID.ToString(), "Cash", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Remove", "-1", "Cash", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("SalaryPayment", "Cash");

        }

        [AllowAnonymous]
        public ActionResult SalaryDetail(Guid? id)
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

            model.SalaryDetail = Db.VDocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.SalaryDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

    }
}