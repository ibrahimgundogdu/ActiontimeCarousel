using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashCollections = Db.VDocumentCashCollections.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
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
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashCollect != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashCollect.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashCollect.LocationID, cashCollect.Currency);
                if (amount > 0)
                {
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
                    collection.ReferanceID = cashCollect.ReferanceID;
                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddCashCollection(collection, model.Authentication);

                    result.Message = addresult.Message;
                    result.IsSuccess = addresult.IsSuccess;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }


            TempData["result"] = result;

            return RedirectToAction("Index", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashCollection(NewCashCollect cashCollect)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashCollect != null)
            {
                var fromPrefix = cashCollect.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.FromID.Substring(1, cashCollect.FromID.Length - 1));
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                double? newexchanges = Convert.ToDouble(cashCollect.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashCollect.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }

                if (amount > 0)
                {
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
                    collection.ReferanceID = cashCollect.ReferanceID;
                    collection.UID = cashCollect.UID;
                    if (newexchanges > 0)
                    {
                        collection.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        collection.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditCashCollection(collection, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }


            TempData["result"] = result;

            return RedirectToAction("CashDetail", new { id = cashCollect.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashCollection(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashCollection(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;

            return RedirectToAction("CashDetail", new { id = id });
        }

        [AllowAnonymous]
        public ActionResult CashDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

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
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashSales = Db.VDocumentTicketSales.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashSales = model.CashSales.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterSale(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("Sale", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashSale(NewCashSale cashSale)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashSale != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSale.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSale.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSale.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSale.FromID.Substring(1, cashSale.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSale.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashSale.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSale.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSale.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSale.LocationID, cashSale.Currency);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var exchange = OfficeHelper.GetExchange(docDate);

                if (amount > 0 && (int?)cashSale.Quantity > 0)
                {
                    CashSale sale = new CashSale();

                    sale.ActinTypeID = actType.ID;
                    sale.ActionTypeName = actType.Name;
                    sale.Amount = amount;
                    sale.Quantity = cashSale.Quantity;
                    sale.CashID = cash.ID;
                    sale.Currency = currency;
                    sale.DocumentDate = docDate;
                    sale.Description = cashSale.Description;
                    sale.PayMethodID = cashSale.PayMethodID;
                    sale.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    sale.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    sale.LocationID = cashSale.LocationID;
                    sale.OurCompanyID = location.OurCompanyID;
                    sale.TimeZone = timezone;
                    sale.ReferanceID = cashSale.ReferanceID;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddCashSale(sale, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Sale", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashSale(NewCashSale cashSale)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashSale != null)
            {
                var fromPrefix = cashSale.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSale.FromID.Substring(1, cashSale.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSale.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var quantity = Convert.ToInt32(cashSale.Quantity);
                var currency = cashSale.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSale.LocationID);
                double? newexchanges = Convert.ToDouble(cashSale.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashSale.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(cashSale.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSale.DocumentDate).Date;
                }
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                if (amount > 0 && quantity > 0)
                {
                    CashSale sale = new CashSale();
                    sale.ActinTypeID = cashSale.ActinTypeID;
                    sale.Amount = amount;
                    sale.Currency = currency;
                    sale.Description = cashSale.Description;
                    sale.DocumentDate = docDate;
                    sale.EnvironmentID = 2;
                    sale.FromCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    sale.LocationID = cashSale.LocationID;
                    sale.UID = cashSale.UID;
                    sale.Quantity = quantity;
                    sale.PayMethodID = cashSale.PayMethodID;
                    sale.ReferanceID = cashSale.ReferanceID;
                    sale.TimeZone = timezone;
                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditCashSale(sale, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;
            return RedirectToAction("SaleDetail", new { id = cashSale.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashSale(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashSale(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;

            return RedirectToAction("SaleDetail", new { id = id });

        }

        [AllowAnonymous]
        public ActionResult SaleDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

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
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            var ourCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompany.CompanyID);
            model.CurrencyList = OfficeHelper.GetCurrency().Where(x => x.Code != ourCompany.Currency).ToList();

            model.CashSaleExchanges = Db.VDocumentSaleExchange.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashSaleExchanges = model.CashSaleExchanges.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterExchange(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("Exchange", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashExchange(NewCashExchange cashSale, HttpPostedFileBase documentFile)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashSale != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSale.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSale.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var amount = Convert.ToDouble(cashSale.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var currencyo = cashSale.Currency;
                var currencyi = location.Currency != null ? location.Currency : ourcompany.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSale.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSale.DocumentDate).Date;
                }
                var casho = OfficeHelper.GetCash(cashSale.LocationID, currencyo);
                var cashi = OfficeHelper.GetCash(cashSale.LocationID, currencyi);

                string path = Server.MapPath("/");


                var exchange = OfficeHelper.GetExchange(docDate);
                var exchangerate = Convert.ToDouble(cashSale.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                if (amount > 0)
                {
                    SaleExchange sale = new SaleExchange();

                    sale.ActinTypeID = actType.ID;
                    sale.ActionTypeName = actType.Name;
                    sale.FromCashID = casho.ID;
                    sale.Amount = amount;
                    sale.Currency = currencyo;

                    sale.SaleExchangeRate = exchangerate;
                    sale.ToCashID = cashi.ID;
                    sale.ToAmount = (amount * exchangerate);
                    sale.ToCurrency = currencyi;

                    sale.Description = cashSale.Description;
                    sale.DocumentDate = docDate;
                    sale.LocationID = cashSale.LocationID;

                    sale.ExchangeRate = exchangerate > 0 ? exchangerate : currencyo == "USD" ? exchange.USDA : currencyo == "EUR" ? exchange.EURA : 1;
                    sale.OurCompanyID = location.OurCompanyID;
                    sale.TimeZone = timezone;
                    sale.ReferanceID = cashSale.ReferanceID;

                    sale.SlipPath = "";
                    sale.SlipDocument = "";
                    sale.TimeZone = timezone;


                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        sale.SlipDocument = filename;
                        sale.SlipPath = "/Document/Exchange";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(sale.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddSaleExchange(sale, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Exchange", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashExchange(NewCashExchange cashCollect, HttpPostedFileBase documentFile)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashCollect != null)
            {
                var amount = Convert.ToDouble(cashCollect.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashCollect.LocationID);
                double? newexchanges = Convert.ToDouble(cashCollect.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashCollect.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var saleExchange = Convert.ToDouble(cashCollect.Exchange.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }

                if (amount > 0)
                {
                    SaleExchange sale = new SaleExchange();
                    sale.ActinTypeID = cashCollect.ActinTypeID;
                    sale.Amount = amount;
                    sale.Currency = currency;
                    sale.Description = cashCollect.Description;
                    sale.DocumentDate = docDate;
                    sale.EnvironmentID = 2;
                    sale.LocationID = cashCollect.LocationID;
                    sale.UID = cashCollect.UID;
                    sale.SaleExchangeRate = saleExchange;
                    sale.ReferanceID = cashCollect.ReferanceID;
                    sale.TimeZone = timezone;
                    //sale.SlipPath = "";
                    //sale.SlipDocument = "";

                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        sale.SlipDocument = filename;
                        sale.SlipPath = "/Document/Exchange";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(sale.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditSaleExchange(sale, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;

                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("ExchangeDetail", new { id = cashCollect.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashExchange(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteSaleExchange(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;

            return RedirectToAction("ExchangeDetail", new { id = id });

        }

        [AllowAnonymous]
        public ActionResult ExchangeDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

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
                model.Result = TempData["result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.DateBegin = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashOpenSlip = Db.VDocumentCashOpen.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashOpenSlip = model.CashOpenSlip.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }
            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterOpen(int? locationId, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.LocationID = locationId;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;

            if (beginDate == null)
            {
                DateTime begin = DateTime.Now.AddYears(-1).Date;
                model.DateBegin = new DateTime(begin.Year, 1, 1);
            }

            if (endDate == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Open", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashOpen(NewCashOpen cashOpen)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashOpen != null)
            {

                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashOpen.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashOpen.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var amount = Convert.ToDouble(cashOpen.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashOpen.Currency;
                var cash = OfficeHelper.GetCash(cashOpen.LocationID, cashOpen.Currency);
                var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                int docYear = cashOpen.docDate ?? DateTime.Now.Year;
                var docDate = new DateTime(docYear, 1, 1);

                if (amount > 0)
                {
                    CashOpen open = new CashOpen();

                    open.ActinTypeID = actType.ID;
                    open.ActionTypeName = actType.Name;
                    open.Amount = amount;
                    open.Currency = currency;
                    open.Description = cashOpen.Description;
                    open.DocumentDate = docDate;
                    open.docDate = docYear;
                    open.LocationID = cashOpen.LocationID;
                    open.CashID = cash.ID;
                    open.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    open.OurCompanyID = location.OurCompanyID;
                    open.TimeZone = timezone;
                    open.ReferanceID = cashOpen.ReferanceID;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddCashOpen(open, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Open", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashOpen(NewCashOpen cashOpen)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashOpen != null)
            {

                var amount = Convert.ToDouble(cashOpen.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashOpen.Currency;
                double? newexchanges = Convert.ToDouble(cashOpen.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashOpen.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashOpen.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                //var docDate = DateTime.Now.Date;
                //if (DateTime.TryParse(cashOpen.DocumentDate, out docDate))
                //{
                //    docDate = Convert.ToDateTime(cashOpen.DocumentDate).Date;
                //}

                if (amount > 0)
                {
                    CashOpen sale = new CashOpen();
                    sale.ActinTypeID = cashOpen.ActinTypeID;
                    sale.Amount = amount;
                    sale.Currency = currency;
                    sale.Description = cashOpen.Description;
                    sale.EnvironmentID = 2;
                    sale.LocationID = cashOpen.LocationID;
                    sale.UID = cashOpen.UID;
                    //sale.DocumentDate = docDate;
                    sale.ReferanceID = cashOpen.ReferanceID;
                    sale.TimeZone = timezone;
                    //sale.docDate = cashOpen.docDate ?? DateTime.Now.Year;
                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditCashOpen(sale, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("OpenDetail", new { id = cashOpen.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashOpen(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashOpen(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("OpenDetail", new { id = id });

        }

        [AllowAnonymous]
        public ActionResult OpenDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.OpenDetail = Db.VDocumentCashOpen.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "Open" && x.Environment == "Office" && x.ProcessID == model.OpenDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }



        [AllowAnonymous]
        public ActionResult CashPayment(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashPayments = Db.VDocumentCashPayments.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashPayments = model.CashPayments.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A" || x.Prefix == "E").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterPayment(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("CashPayment", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashPayment(NewCashPayments cashPayment)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashPayment != null)
            {

                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashPayment.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashPayment.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashPayment.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashPayment.FromID.Substring(1, cashPayment.FromID.Length - 1));
                var amount = Convert.ToDouble(cashPayment.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashPayment.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;


                if (DateTime.TryParse(cashPayment.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashPayment.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashPayment.LocationID, cashPayment.Currency);
                // tahsilat eklenir.
                var exchange = OfficeHelper.GetExchange(docDate);

                if (amount > 0)
                {
                    CashPayment payment = new CashPayment();

                    payment.ActinTypeID = actType.ID;
                    payment.ActionTypeName = actType.Name;
                    payment.Amount = amount;
                    payment.CashID = cash.ID;
                    payment.Currency = currency;
                    payment.DocumentDate = docDate;
                    payment.Description = cashPayment.Description;

                    payment.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    payment.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                    payment.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    payment.LocationID = cashPayment.LocationID;
                    payment.OurCompanyID = location.OurCompanyID;
                    payment.TimeZone = timezone;
                    payment.ReferanceID = cashPayment.ReferanceID;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddCashPayment(payment, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("CashPayment", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashPayment(NewCashPayments cashPayment)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };
            CashControlModel model = new CashControlModel();


            if (cashPayment != null)
            {
                var fromPrefix = cashPayment.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashPayment.FromID.Substring(1, cashPayment.FromID.Length - 1));
                var amount = Convert.ToDouble(cashPayment.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashPayment.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashPayment.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(cashPayment.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashPayment.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(cashPayment.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashPayment.DocumentDate).Date;
                }

                if (amount > 0)
                {
                    CashPayment payment = new CashPayment();
                    payment.ActinTypeID = cashPayment.ActinTypeID;
                    payment.Amount = amount;
                    payment.Currency = currency;
                    payment.Description = cashPayment.Description;
                    payment.DocumentDate = docDate;
                    payment.EnvironmentID = 2;
                    payment.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    payment.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                    payment.LocationID = cashPayment.LocationID;
                    payment.UID = cashPayment.UID;
                    payment.ReferanceID = cashPayment.ReferanceID;
                    payment.TimeZone = timezone;
                    if (newexchanges > 0)
                    {
                        payment.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        payment.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditCashPayment(payment, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;
            return RedirectToAction("PaymentDetail", new { id = cashPayment.UID });
        }

        [AllowAnonymous]
        public ActionResult DeleteCashPayment(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashPayment(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("PaymentDetail", new { id = id });

        }

        [AllowAnonymous]
        public ActionResult PaymentDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

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
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.TicketSalesReturn = Db.VDocumentTicketSaleReturn.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.TicketSalesReturn = model.TicketSalesReturn.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterReturn(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("SaleReturn", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddTicketSaleReturn(NewTicketSaleReturn cashSaleReturn)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashSaleReturn != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSaleReturn.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSaleReturn.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSaleReturn.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSaleReturn.FromID.Substring(1, cashSaleReturn.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSaleReturn.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashSaleReturn.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSaleReturn.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSaleReturn.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSaleReturn.LocationID, cashSaleReturn.Currency);

                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var exchange = OfficeHelper.GetExchange(docDate);

                if (amount > 0 && (int?)cashSaleReturn.Quantity > 0)
                {
                    SaleReturn sale = new SaleReturn();

                    sale.ActinTypeID = actType.ID;
                    sale.ActionTypeName = actType.Name;
                    sale.Amount = amount;
                    sale.Quantity = cashSaleReturn.Quantity;
                    sale.CashID = cash.ID;
                    sale.Currency = currency;
                    sale.DocumentDate = docDate;
                    sale.Description = cashSaleReturn.Description;
                    sale.PayMethodID = cashSaleReturn.PayMethodID;
                    sale.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    sale.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    sale.LocationID = cashSaleReturn.LocationID;
                    sale.OurCompanyID = location.OurCompanyID;
                    sale.TimeZone = timezone;
                    sale.ReferanceID = cashSaleReturn.ReferanceID;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddCashSaleReturn(sale, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("SaleReturn", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditTicketSaleReturn(NewTicketSaleReturn cashSale)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashSale != null)
            {
                var fromPrefix = cashSale.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSale.FromID.Substring(1, cashSale.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSale.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var quantity = Convert.ToInt32(cashSale.Quantity);
                var currency = cashSale.Currency;
                var docDate = DateTime.Now.Date;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSale.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(cashSale.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashSale.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (DateTime.TryParse(cashSale.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSale.DocumentDate).Date;
                }

                if (amount > 0 && quantity > 0)
                {
                    SaleReturn sale = new SaleReturn();
                    sale.ActinTypeID = cashSale.ActinTypeID;
                    sale.Amount = amount;
                    sale.Currency = currency;
                    sale.Description = cashSale.Description;
                    sale.DocumentDate = docDate;
                    sale.EnvironmentID = 2;
                    sale.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    sale.LocationID = cashSale.LocationID;
                    sale.UID = cashSale.UID;
                    sale.Quantity = quantity;
                    sale.PayMethodID = cashSale.PayMethodID;
                    sale.ReferanceID = cashSale.ReferanceID;
                    sale.TimeZone = timezone;
                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditCashSaleReturn(sale, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }


            TempData["result"] = result;
            return RedirectToAction("SaleRefundDetail", new { id = cashSale.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteTicketSaleReturn(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashSaleReturn(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("SaleRefundDetail", new { id = id });

        }

        [AllowAnonymous]
        public ActionResult SaleRefundDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.SalesRefundDetail = Db.VDocumentTicketSaleReturn.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "SaleReturn" && x.Environment == "Office" && x.ProcessID == model.SalesRefundDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "A").ToList();

            return View(model);
        }



        [AllowAnonymous]
        public ActionResult Expense(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.CashExpense = Db.VDocumentCashExpense.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashExpense = model.CashExpense.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterExpense(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("Expense", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashExpense(NewCashExpense cashExpense, HttpPostedFileBase documentFile)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashExpense != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashExpense.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashExpense.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashExpense.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashExpense.FromID.Substring(1, cashExpense.FromID.Length - 1));
                var amount = Convert.ToDouble(cashExpense.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashExpense.Currency;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                var docDate = DateTime.Now.Date;
                var slipDate = DateTime.Now.Date;

                if (DateTime.TryParse(cashExpense.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashExpense.DocumentDate).Date;
                }

                if (DateTime.TryParse(cashExpense.SlipDate, out slipDate))
                {
                    slipDate = Convert.ToDateTime(cashExpense.SlipDate).Date;
                }

                TimeSpan? time = Convert.ToDateTime(cashExpense.SlipTime).TimeOfDay;
                DateTime? slipdatetime = slipDate.Add(time.Value);
                var cash = OfficeHelper.GetCash(cashExpense.LocationID, cashExpense.Currency);


                var exchange = OfficeHelper.GetExchange(docDate);


                if (amount > 0)
                {
                    CashExpense expense = new CashExpense();

                    expense.ActinTypeID = actType.ID;
                    expense.ActionTypeName = actType.Name;
                    expense.Amount = amount;
                    expense.CashID = cash.ID;
                    expense.Currency = currency;
                    expense.DocumentDate = docDate;
                    expense.Description = cashExpense.Description;

                    expense.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    expense.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                    expense.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                    expense.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    expense.LocationID = cashExpense.LocationID;
                    expense.OurCompanyID = location.OurCompanyID;
                    expense.TimeZone = timezone;
                    expense.SlipNumber = cashExpense.SlipNumber;
                    expense.ReferanceID = cashExpense.ReferanceID;
                    expense.ExpenseTypeID = cashExpense.ExpenseTypeID ?? (int?)null;
                    expense.SlipDate = slipdatetime;
                    expense.SlipPath = "";
                    expense.SlipDocument = "";

                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        expense.SlipDocument = filename;
                        expense.SlipPath = "/Document/Expense";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(expense.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }


                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddCashExpense(expense, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("Expense", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashExpense(NewCashExpense cashExpense, HttpPostedFileBase documentFile)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashExpense != null)
            {
                var fromPrefix = cashExpense.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashExpense.FromID.Substring(1, cashExpense.FromID.Length - 1));
                var amount = Convert.ToDouble(cashExpense.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashExpense.Currency;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashExpense.LocationID);

                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(cashExpense.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashExpense.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var docDate = DateTime.Now.Date;
                var slipDate = DateTime.Now.Date;

                if (DateTime.TryParse(cashExpense.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashExpense.DocumentDate).Date;
                }

                if (DateTime.TryParse(cashExpense.SlipDate, out slipDate))
                {
                    slipDate = Convert.ToDateTime(cashExpense.SlipDate).Date;
                }

                TimeSpan? time = Convert.ToDateTime(cashExpense.SlipTime).TimeOfDay;
                DateTime? slipdatetime = slipDate.Add(time.Value);

                if (amount > 0)
                {
                    CashExpense sale = new CashExpense();
                    sale.ActinTypeID = cashExpense.ActinTypeID;
                    sale.Amount = amount;
                    sale.Currency = currency;
                    sale.Description = cashExpense.Description;
                    sale.DocumentDate = docDate;
                    sale.EnvironmentID = 2;
                    sale.ToCustomerID = fromPrefix == "A" ? fromID : (int?)null;
                    sale.ToBankAccountID = fromPrefix == "B" ? fromID : (int?)null;
                    sale.ToEmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                    sale.LocationID = cashExpense.LocationID;
                    sale.UID = cashExpense.UID;
                    sale.ReferanceID = cashExpense.ReferanceID;
                    sale.TimeZone = timezone;
                    sale.ExpenseTypeID = cashExpense.ExpenseTypeID ?? (int?)null;
                    sale.SlipNumber = cashExpense.SlipNumber;
                    sale.SlipDate = slipdatetime;
                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        sale.SlipDocument = filename;
                        sale.SlipPath = "/Document/Expense";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(sale.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditCashExpense(sale, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;
            return RedirectToAction("ExpenseDetail", new { id = cashExpense.UID });
        }

        [AllowAnonymous]
        public ActionResult DeleteCashExpense(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashExpense(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("ExpenseDetail", new { id = id });
        }

        [AllowAnonymous]
        public ActionResult ExpenseDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

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
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.BankTransfer = Db.VDocumentBankTransfer.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.BankTransfer = model.BankTransfer.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "B").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterBank(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("BankTransfer", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashBankTransfer(NewCashBankTransfer cashTransfer, HttpPostedFileBase documentFile)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashTransfer != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashTransfer.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashTransfer.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashTransfer.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashTransfer.FromID.Substring(1, cashTransfer.FromID.Length - 1));
                var amount = Convert.ToDouble(cashTransfer.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var commision = !string.IsNullOrEmpty(cashTransfer.Commission) ? Convert.ToDouble(cashTransfer.Commission.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : 0;
                var currency = cashTransfer.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                //long? refID = cashTransfer.ReferanceID ?? (long?)null;

                DateTime? date = Convert.ToDateTime(cashTransfer.SlipDate);
                TimeSpan? time = Convert.ToDateTime(cashTransfer.SlipTime).TimeOfDay;
                DateTime? slipdatetime = date.Value.Add(time.Value);

                if (DateTime.TryParse(cashTransfer.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashTransfer.DocumentDate).Date;
                }


                var cash = OfficeHelper.GetCash(cashTransfer.LocationID, cashTransfer.Currency);
                var exchange = OfficeHelper.GetExchange(docDate);

                if (amount > 0)
                {
                    DocumentManager documentManager = new DocumentManager();
                    string path = Server.MapPath("/");

                    BankTransfer bankTransfer = new BankTransfer();

                    bankTransfer.ActinTypeID = actType.ID;
                    bankTransfer.ActionTypeName = actType.Name;
                    bankTransfer.Amount = amount;
                    bankTransfer.Commission = commision;
                    bankTransfer.Currency = currency;
                    bankTransfer.Description = cashTransfer.Description;
                    bankTransfer.DocumentDate = docDate;
                    bankTransfer.EmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    bankTransfer.EnvironmentID = 2;
                    bankTransfer.ExchangeRate = currency == "USD" ? exchange.USDA.Value : currency == "EUR" ? exchange.EURA.Value : 1;
                    bankTransfer.FromCashID = cash.ID;
                    bankTransfer.LocationID = location.LocationID;
                    bankTransfer.OurCompanyID = location.OurCompanyID;
                    bankTransfer.SlipDate = slipdatetime;
                    bankTransfer.SlipNumber = cashTransfer.SlipNumber;
                    //bankTransfer.SlipPath = Server.MapPath("/");
                    bankTransfer.TimeZone = timezone;
                    bankTransfer.ToBankID = fromPrefix == "B" ? fromID : (int?)null;
                    bankTransfer.ReferanceID = cashTransfer.ReferanceID;
                    bankTransfer.UID = Guid.NewGuid();
                    bankTransfer.ReferanceModel = "DocumentBankTransfer";
                    bankTransfer.SlipPath = "";
                    bankTransfer.SlipDocument = "";

                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        bankTransfer.SlipDocument = filename;
                        bankTransfer.SlipPath = "/Document/Bank";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(bankTransfer.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }


                    var addresult = documentManager.AddBankTransfer(bankTransfer, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("BankTransfer", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashBankTransfer(NewCashBankTransfer cashTransfer, HttpPostedFileBase documentFile)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashTransfer != null)
            {
                var fromPrefix = cashTransfer.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashTransfer.FromID.Substring(1, cashTransfer.FromID.Length - 1));
                var amount = Convert.ToDouble(cashTransfer.Amount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var commision = !string.IsNullOrEmpty(cashTransfer.Commission) ? Convert.ToDouble(cashTransfer.Commission.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture) : 0;
                var currency = cashTransfer.Currency;
                bool? isActive = !string.IsNullOrEmpty(cashTransfer.IsActive) ? true : false;
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashTransfer.LocationID);

                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                double? newexchanges = Convert.ToDouble(cashTransfer.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashTransfer.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var docDate = DateTime.Now.Date;
                var slipDate = DateTime.Now.Date;

                if (DateTime.TryParse(cashTransfer.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashTransfer.DocumentDate).Date;
                }

                if (DateTime.TryParse(cashTransfer.SlipDate, out slipDate))
                {
                    slipDate = Convert.ToDateTime(cashTransfer.SlipDate).Date;
                }

                TimeSpan? time = Convert.ToDateTime(cashTransfer.SlipTime).TimeOfDay;
                DateTime? slipdatetime = slipDate.Add(time.Value);

                if (amount > 0)
                {
                    BankTransfer banktransfer = new BankTransfer();

                    banktransfer.ActinTypeID = cashTransfer.ActinTypeID;
                    banktransfer.Amount = amount;
                    banktransfer.Currency = currency;
                    banktransfer.Description = cashTransfer.Description;
                    banktransfer.DocumentDate = docDate;
                    banktransfer.ToBankID = fromPrefix == "B" ? fromID : (int?)null;
                    banktransfer.LocationID = cashTransfer.LocationID;
                    banktransfer.UID = cashTransfer.UID;
                    banktransfer.Commission = commision;
                    banktransfer.SlipDate = slipdatetime;
                    banktransfer.TrackingNumber = cashTransfer.TrackingNumber;
                    banktransfer.ReferanceCode = cashTransfer.ReferenceCode;
                    banktransfer.StatusID = cashTransfer.StatusID;
                    banktransfer.IsActive = isActive;
                    banktransfer.SlipNumber = cashTransfer.SlipNumber;
                    //banktransfer.SlipPath = cashTransfer.Slip;
                    banktransfer.ReferanceID = cashTransfer.ReferanceID;
                    banktransfer.TimeZone = timezone;
                    //banktransfer.SlipDocument = "";

                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        banktransfer.SlipDocument = filename;
                        banktransfer.SlipPath = "/Document/Bank";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(banktransfer.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    if (newexchanges > 0)
                    {
                        banktransfer.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        banktransfer.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditBankTransfer(banktransfer, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;
            return RedirectToAction("TransferDetail", new { id = cashTransfer.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashBankTransfer(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteCashBankTransfer(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("TransferDetail", new { id = id });
        }

        [AllowAnonymous]
        public ActionResult TransferDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.TransferDetail = Db.VDocumentBankTransfer.FirstOrDefault(x => x.UID == id);

            if (model.TransferDetail != null)
            {
                model.History = Db.ApplicationLog.Where(x => x.Controller == "Cash" && x.Action == "BankTransfer" && x.Environment == "Office" && x.ProcessID == model.TransferDetail.ID.ToString()).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "B").ToList();

            return View(model);
        }






        [AllowAnonymous]
        public ActionResult SalaryPayment(int? locationId)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
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
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.SalaryPayment = Db.VDocumentSalaryPayment.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.SalaryPayment = model.SalaryPayment.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult NewSalaryPayment()
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
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


            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.SalaryTypes = Db.SalaryType.Where(x => x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID ).OrderBy(x => x.FullName).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.SalaryPayment = Db.VDocumentSalaryPayment.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.SalaryPayment = model.SalaryPayment.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }


            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterSalary(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("SalaryPayment", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashSalaryPayment(NewCashSalaryPayment cashSalary)
        {

            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashSalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSalary.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSalary.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSalary.FromID.Substring(1, cashSalary.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashSalary.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;


                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                var exchange = OfficeHelper.GetExchange(docDate);

                var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
                // tahsilat eklenir.
                if (amount > 0)
                {
                    SalaryPayment payment = new SalaryPayment();

                    payment.ActinTypeID = actType.ID;
                    payment.ActionTypeName = actType.Name;
                    payment.Currency = cashSalary.Currency;
                    payment.Description = cashSalary.Description;
                    payment.DocumentDate = docDate;
                    payment.EmployeeID = fromPrefix == "E" ? fromID : (int)0; ;
                    payment.EnvironmentID = 2;
                    payment.LocationID = location.LocationID;
                    payment.Amount = amount;
                    payment.UID = Guid.NewGuid();
                    payment.TimeZone = timezone;
                    payment.OurCompanyID = location.OurCompanyID;
                    payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
                    payment.FromBankID = (int?)cashSalary.BankAccountID > 0 ? cashSalary.BankAccountID : (int?)null;
                    payment.FromCashID = (int?)cashSalary.BankAccountID == 0 ? cash.ID : (int?)null;
                    payment.SalaryTypeID = cashSalary.SalaryType;
                    payment.ReferanceID = cashSalary.ReferanceID;
                    payment.CategoryID = cashSalary.CategoryID ?? (int?)null;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddSalaryPayment(payment, model.Authentication);


                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }

            TempData["result"] = result;

            return RedirectToAction("SalaryPayment", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashSalaryPayment(CashSalaryPayment cashsalary)
        {

            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashsalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 31);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashsalary.LocationID);
                var amount = Convert.ToDouble(cashsalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                int timezone = location.Timezone.Value;

                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashsalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashsalary.DocumentDate).Date;
                }

                var exchange = OfficeHelper.GetExchange(docDate);

                var cash = OfficeHelper.GetCash(cashsalary.LocationID, cashsalary.Currency);

                if (amount > 0)
                {
                    SalaryPayment payment = new SalaryPayment();

                    payment.ActinTypeID = actType.ID;
                    payment.ActionTypeName = actType.Name;
                    payment.Currency = cashsalary.Currency;
                    payment.Description = cashsalary.Description;
                    payment.DocumentDate = docDate;
                    payment.EmployeeID = cashsalary.EmployeeID;
                    payment.EnvironmentID = 2;
                    payment.LocationID = location.LocationID;
                    payment.Amount = amount;
                    payment.UID = Guid.NewGuid();
                    payment.TimeZone = timezone;
                    payment.OurCompanyID = location.OurCompanyID;
                    payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
                    payment.FromBankID = (int?)null;
                    payment.FromCashID = cash.ID;
                    payment.SalaryTypeID = cashsalary.SalaryTypeID;
                    payment.CategoryID = cashsalary.CategoryID;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddSalaryPayment(payment, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            TempData["result"] = result;

            return RedirectToAction("SalaryPayment", "Cash");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryPayment(NewCashSalaryPayment cashSalary)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (cashSalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSalary.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var fromPrefix = cashSalary.FromID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSalary.FromID.Substring(1, cashSalary.FromID.Length - 1));
                var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashSalary.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                double? newexchanges = Convert.ToDouble(cashSalary.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? exchanges = Convert.ToDouble(cashSalary.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                if (amount > 0)
                {
                    SalaryPayment sale = new SalaryPayment();
                    sale.LocationID = cashSalary.ActinTypeID;
                    sale.Currency = cashSalary.Currency;
                    sale.DocumentDate = docDate;
                    sale.EmployeeID = fromPrefix == "E" ? fromID : (int)0;
                    sale.Amount = amount;
                    sale.Description = cashSalary.Description;
                    sale.FromBankID = cashSalary.BankAccountID;
                    sale.SalaryTypeID = cashSalary.SalaryType;
                    sale.UID = cashSalary.UID;
                    sale.CategoryID = cashSalary.CategoryID;
                    sale.ReferanceID = cashSalary.ReferanceID;
                    sale.TimeZone = timezone;
                    sale.FromCashID = cashSalary.CashID;

                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditSalaryPayment(sale, model.Authentication);

                    result.IsSuccess = editresult.IsSuccess;
                    result.Message = editresult.Message;
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }

            TempData["result"] = result;
            return RedirectToAction("SalaryDetail", new { id = cashSalary.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteSalaryPayment(string id)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                var delresult = documentManager.DeleteSalaryPayment(Guid.Parse(id), model.Authentication);

                result.IsSuccess = delresult.IsSuccess;
                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("SalaryDetail", new { id = id });
        }

        [AllowAnonymous]
        public ActionResult SalaryDetail(Guid? id)
        {
            CashControlModel model = new CashControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }
            model.ExpenseTypeList = Db.ExpenseType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.SalaryTypes = Db.SalaryType.Where(x => x.IsActive == true).ToList();
            model.BankAccountList = Db.BankAccount.ToList();
            model.PayMethodList = Db.PayMethod.ToList();
            model.StatusList = Db.BankTransferStatus.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();

            model.SalaryDetail = Db.VDocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
            var ll = new string[] { "Cash", "Salary" };
            //var ac = new string[] { "", "SalaryPayment" };
            model.History = Db.ApplicationLog.Where(x => ll.Contains(x.Controller) && x.Action == "SalaryPayment" && x.Environment == "Office" && x.ProcessID == model.SalaryDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

    }
}