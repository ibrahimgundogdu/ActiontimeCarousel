using ActionForce.Entity;
using System;
using System.Collections.Generic;
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
                    newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                    newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                    newCashColl.SystemAmount = ourcompany.Currency == currency ? amount : amount * newCashColl.ExchangeRate;
                    newCashColl.SystemCurrency = ourcompany.Currency;


                    Db.DocumentCashCollections.Add(newCashColl);
                    Db.SaveChanges();

                    // cari hesap işlemesi
                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Kasa Tahsilatı başarı ile eklendi";

                    // log atılır
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


                    Db.DocumentTicketSales.Add(newCashColl);
                    Db.SaveChanges();

                    OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Bilet satışı başarı ile eklendi";

                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "Sale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                }
                catch (Exception ex)
                {
                    result.Message = $"Bilet satışı eklenemedi : {ex.Message}";
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

                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "ExchangeSale", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{amount} {currencyo} kasa döviz satışı eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                    }
                }
                else
                {
                    result.Message = $"{amount} {currencyo} kasa döviz satışı eklenemedi. Kasa bakiyesi bu tutar için  yetersiz";
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "ExchangeSale", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Exchange", "Cash");
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
                    var docDate = new DateTime(DateTime.Now.Year,1,1);

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

                        Db.DocumentCashOpen.Add(newCashColl);
                        Db.SaveChanges();

                        OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, 1, newCashColl.Amount, 0, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi başarı ile eklendi";

                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", newCashColl.ID.ToString(), "Cash", "CashOpen", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{amount} {currency} tutarındaki kasa açılış fişi eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Cash", "Insert", "-1", "Cash", "CashOpen", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                    }

                }
                else
                {
                    // Update edilir.

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
                        UpdateIP = isOpen.UpdateIP
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
                    OfficeHelper.AddApplicationLog("Office", "Cash", "Update", isOpen.ID.ToString(), "Cash", "CashOpen", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty);
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Open", "Cash");
        }

    }
}