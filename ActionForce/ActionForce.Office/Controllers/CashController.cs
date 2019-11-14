using ActionForce.Entity;
using System;
using System.Collections.Generic;
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
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashCollect.LocationID, cashCollect.Currency);
                // tahsilat eklenir.

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
                    newCashColl.RecordDate = DateTime.UtcNow.AddHours(3);
                    newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                    newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                    newCashColl.SystemCurrency = ourcompany.Currency;


                    Db.DocumentCashCollections.Add(newCashColl);
                    Db.SaveChanges();

                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Kasa Tahsilatı başarı ile eklendi";
                    result.Data = newCashColl;

                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }
                catch (Exception ex)
                {
                    result.Message = $"Kasa Tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Cash");
        }

        [AllowAnonymous]
        public ActionResult Sale(int? locationId)
        {
            CashControlModel model = new CashControlModel();

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

            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashSales = Db.VDocumentTicketSales.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashSales = model.CashSales.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x=> x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashSale(NewCashSale cashSale)
        {
            Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
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


                    Db.DocumentTicketSales.Add(newCashColl);
                    Db.SaveChanges();

                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Kasa Tahsilatı başarı ile eklendi";
                    result.Data = newCashColl;

                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }
                catch (Exception ex)
                {
                    result.Message = $"Kasa Tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }
            }

            TempData["result"] = result;

            return RedirectToAction("Sale", "Cash");
        }

        [AllowAnonymous]
        public ActionResult Exchange(int? locationId)
        {
            CashControlModel model = new CashControlModel();

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

            
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            var ourCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.CurrentLocation.OurCompanyID);
            model.CurrencyList = OfficeHelper.GetCurrency().Where(x => x.Code != ourCompany.Currency).ToList();

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
        public ActionResult AddCashExchange(NewCashSale cashSale)
        {
            Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
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
                    newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "EXS");
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


                    Db.DocumentTicketSales.Add(newCashColl);
                    Db.SaveChanges();

                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Kasa Tahsilatı başarı ile eklendi";
                    result.Data = newCashColl;

                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }
                catch (Exception ex)
                {
                    result.Message = $"Kasa Tahsilatı eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "Sale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }
            }

            TempData["result"] = result;

            return RedirectToAction("Sale", "Cash");
        }



    }
}