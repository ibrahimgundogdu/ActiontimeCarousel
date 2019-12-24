using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ResultController : BaseController
    {
        // GET: Result
        [AllowAnonymous]
        public ActionResult Envelope(string date)
        {
            ResultControlModel model = new ResultControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<DayResult> ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.LocationScheduleList = Db.VLocationSchedule.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ShiftDate == datekey.DateKey && x.StatusID == 2).ToList();
            model.LocationShiftList = Db.VLocationShift.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ShiftDate == datekey.DateKey).ToList();

            model.DayResultList = Db.VDayResult.Where(x => x.Date == datekey.DateKey).ToList();


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult New(string id, int? locationID, string date)
        {
            ResultControlModel model = new ResultControlModel();

            if ((locationID > 0 && !string.IsNullOrEmpty(date)) || !string.IsNullOrEmpty(id))
            {
                var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

                DateTime.TryParse(date, out _date);

                var currentResult = Db.VDayResult.FirstOrDefault(x => x.UID.ToString() == id || (x.LocationID == locationID && x.Date == _date));

                if (currentResult != null)
                {
                    // yönlendir
                    return RedirectToAction("Detail", new { id = currentResult.UID.ToString() });
                }
                else
                {
                    // oluştur ve yönlendir.

                    DayResult dayresult = new DayResult();

                    dayresult.Date = _date;
                    dayresult.EnvironmentID = 2;
                    dayresult.IsActive = true;
                    dayresult.IsMobile = false;
                    dayresult.LocationID = locationID.Value;
                    dayresult.RecordDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
                    dayresult.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    dayresult.RecordIP = OfficeHelper.GetIPAddress();
                    dayresult.StateID = 1;
                    dayresult.StatusID = null;
                    dayresult.UID = Guid.NewGuid();

                    Db.DayResult.Add(dayresult);
                    Db.SaveChanges();

                    // Itemleri ekle
                    var result = OfficeHelper.AddItemsToResultEnvelope(dayresult.ID);







                    return RedirectToAction("Detail", new { id = dayresult.UID.ToString() });
                }

            }
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Detail(string id, int? locationID, string date)
        {
            ResultControlModel model = new ResultControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<DayResult> ?? null;
            }

            if (locationID > 0 && !string.IsNullOrEmpty(date))
            {
                DateTime? urldate = Convert.ToDateTime(date).Date;
                model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.LocationID == locationID && x.Date == urldate);
                model.DayResult = Db.DayResult.FirstOrDefault(x => x.LocationID == locationID && x.Date == urldate);
            }
            else if (!string.IsNullOrEmpty(id))
            {
                model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.UID.ToString() == id);
                model.DayResult = Db.DayResult.FirstOrDefault(x => x.UID.ToString() == id);
            }

            if (model.DayResult != null)
            {
                model.DayResultItems = Db.DayResultItems.ToList();
                model.DayResultItemList = Db.VDayResultItemList.Where(x => x.ResultID == model.DayResult.ID).ToList();
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.DocumentTypes = Db.DocumentType.Where(x => x.IsActive == true).ToList();
                model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
                model.BankActionTypes = Db.BankActionType.Where(x => x.IsActive == true).ToList();
                model.ExpenseTypes = Db.ExpenseType.Where(x => x.IsActive == true && x.IsLocation == true).ToList();
                model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == model.DayResult.LocationID);
                model.Exchanges = Db.VDocumentSaleExchange.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();
                model.Expenses = Db.VDocumentCashExpense.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();
                model.BankTransfers = Db.VDocumentBankTransfer.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();

                var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == model.DayResult.Date);
                model.CurrentDate = datekey;
                model.TodayDateCode = DateTime.UtcNow.AddHours(model.CurrentLocation.Timezone.Value).Date.ToString("yyyy-MM-dd");
                model.CurrentDateCode = datekey.DateKey.ToString("yyyy-MM-dd");
                model.PrevDateCode = datekey.DateKey.AddDays(-1).Date.ToString("yyyy-MM-dd");
                model.NextDateCode = datekey.DateKey.AddDays(1).Date.ToString("yyyy-MM-dd");

                model.CashActions = Db.VCashActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();
                model.BankActions = Db.VBankActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();
                model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ProcessDate == model.DayResult.Date).ToList();

                model.CashRecorderSlips = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();
                model.DayResultDocuments = Db.VDayResultDocuments.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();
                model.DayResultDocuments = Db.VDayResultDocuments.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();

                model.ResultStates = Db.ResultState.Where(x => x.IsActive == true).ToList();
                model.Resultstatus = Db.Resultstatus.Where(x => x.IsActive == true).ToList();

                model.CurrencyList = OfficeHelper.GetCurrency();

                model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                var trlCash = OfficeHelper.GetCash(model.DayResult.LocationID, "TRL");
                var usdCash = OfficeHelper.GetCash(model.DayResult.LocationID, "USD");
                var eurCash = OfficeHelper.GetCash(model.DayResult.LocationID, "EUR");

                List <TotalModel> devirtotals = new List<TotalModel>();
                devirtotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Total = Db.GetCashBalance(model.DayResult.LocationID,trlCash.ID, model.DayResult.Date.AddDays(-1) ).FirstOrDefault() ?? 0
                });
                devirtotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Total = Db.GetCashBalance(model.DayResult.LocationID, usdCash.ID, model.DayResult.Date.AddDays(-1)).FirstOrDefault() ?? 0
                });
                devirtotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Total = Db.GetCashBalance(model.DayResult.LocationID, eurCash.ID, model.DayResult.Date.AddDays(-1)).FirstOrDefault() ?? 0
                });

                model.DevirTotal = devirtotals;

            }
            else
            {
                return RedirectToAction("Envelope");
            }

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddResultDocument(long? id, HttpPostedFileBase file, int? typeid, string description)
        {
            Result<DayResultDocuments> result = new Result<DayResultDocuments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();
            string path = Server.MapPath("/");
            result = documentManager.AddResultDocument(id, file, path, typeid, description, model.Authentication);

            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);

            model.DayResultDocuments = Db.VDayResultDocuments.Where(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialResultFiles", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddCashRecorder(long? id, HttpPostedFileBase file, int? typeid, string description, string slipnumber, string slipdate, string sliptime, string slipamount, string sliptotalmount)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();
            string path = Server.MapPath("/");
            result = documentManager.AddCashRecorder(id, file, path, typeid, description, slipnumber, slipdate, sliptime, slipamount, sliptotalmount, model.Authentication);

            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);

            model.CashRecorderSlips = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialCashRecorder", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddSalaryEarn(long? id, int? itemid, int? typeid, int? employeeid, string description, string duration, string unithprice, string totalamount)
        {
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();
            DocumentManager documentManager = new DocumentManager();
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);

            if (!string.IsNullOrEmpty(totalamount))
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == typeid);
                var item = Db.DayResultItemList.FirstOrDefault(x => x.ID == itemid);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
                TimeSpan? sure = Convert.ToDateTime(duration + ":00").TimeOfDay;
                var unitPrice = Convert.ToDouble(unithprice.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var totalAmount = Convert.ToDouble(totalamount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var issuccess = OfficeHelper.CalculateSalaryEarn(id, employeeid.Value, dayresult.Date, dayresult.LocationID, model.Authentication);
            }
            else
            {
                result.Message = "Tutar kısmı boş yada 0'dan küçük olamaz. ";
            }


            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == dayresult.LocationID && x.ProcessDate == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialEmployeeSalary", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddSalaryPayment(long? id, int? itemid, int? typeid, int? employeeid, string description, string amount)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();
            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == typeid);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            var payamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, location.Currency);


            SalaryPayment payment = new SalaryPayment();

            payment.ActinTypeID = typeid.Value;
            payment.ActionTypeName = actType.Name;
            payment.Currency = location.Currency;
            payment.Description = description;
            payment.DocumentDate = dayresult.Date;
            payment.EmployeeID = employeeid.Value;
            payment.EnvironmentID = 2;
            payment.LocationID = dayresult.LocationID;
            payment.OurCompanyID = location.OurCompanyID;
            payment.ResultID = id;
            payment.TimeZone = location.Timezone;
            payment.Amount = payamount;
            payment.UID = Guid.NewGuid();
            payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
            payment.FromCashID = cash.ID;
            payment.SalaryTypeID = 2;
            payment.TimeZone = location.Timezone;


            result = documentManager.AddSalaryPayment(payment, model.Authentication);

            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == dayresult.LocationID && x.ProcessDate == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialEmployeeSalary", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddBankTransfer(long? id, HttpPostedFileBase file, long? itemid, int bankid, string description, string slipnumber, string slipdate, string sliptime, string comission, string amount)
        {
            Result<DocumentBankTransfer> result = new Result<DocumentBankTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 30);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var item = Db.DayResultItemList.FirstOrDefault(x => x.ID == itemid);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            var transamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var transcommission = Convert.ToDouble(comission.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, location.Currency);
            DateTime? slipDatetime = null;

            if (!string.IsNullOrEmpty(slipdate) && !string.IsNullOrEmpty(sliptime))
            {
                DateTime slipDate = Convert.ToDateTime(slipdate).Date;
                slipDatetime = slipDate.Add(Convert.ToDateTime(sliptime).TimeOfDay);
            }


            BankTransfer bankTransfer = new BankTransfer();

            bankTransfer.ActinTypeID = actType.ID;
            bankTransfer.ActionTypeName = actType.Name;
            bankTransfer.Amount = transamount;
            bankTransfer.Commission = transcommission;
            bankTransfer.Currency = location.Currency;
            bankTransfer.Description = description;
            bankTransfer.DocumentDate = dayresult.Date;
            bankTransfer.EmployeeID = model.Authentication.ActionEmployee.EmployeeID;
            bankTransfer.EnvironmentID = 2;
            bankTransfer.ExchangeRate = bankTransfer.Currency == "USD" ? exchange.USDA.Value : bankTransfer.Currency == "EUR" ? exchange.EURA.Value : 1;
            bankTransfer.FromCashID = cash.ID;
            bankTransfer.LocationID = location.LocationID;
            bankTransfer.OurCompanyID = location.OurCompanyID;
            bankTransfer.SlipDate = slipDatetime;
            bankTransfer.SlipNumber = slipnumber;
            bankTransfer.SlipPath = Server.MapPath("/");
            bankTransfer.TimeZone = location.Timezone.Value;
            bankTransfer.ToBankID = bankid;
            bankTransfer.UID = Guid.NewGuid();
            bankTransfer.ResultID = dayresult.ID;
            bankTransfer.StatusID = 3;

            result = documentManager.AddBankTransfer(bankTransfer, file, model.Authentication);

            model.BankTransfers = Db.VDocumentBankTransfer.Where(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialBankTransfer", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddExpense(long? id, long? itemid, HttpPostedFileBase file, int? exptypeid, string amount, string currency, string description, string slipnumber, string slipdate, string sliptime)
        {
            Result<DocumentCashExpense> result = new Result<DocumentCashExpense>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 29);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var item = Db.DayResultItemList.FirstOrDefault(x => x.ID == itemid);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            var expenseamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            //var expensequantity = 1;
            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);
            DateTime? slipDatetime = null;

            if (!string.IsNullOrEmpty(slipdate) && !string.IsNullOrEmpty(sliptime))
            {
                DateTime slipDate = Convert.ToDateTime(slipdate).Date;
                slipDatetime = slipDate.Add(Convert.ToDateTime(sliptime).TimeOfDay);
            }

            CashExpense expense = new CashExpense();

            expense.ActinTypeID = actType.ID;
            expense.ActionTypeName = actType.Name;
            expense.Amount = expenseamount;
            expense.Currency = currency;
            expense.Description = description;
            expense.DocumentDate = dayresult.Date;
            expense.EnvironmentID = 2;
            expense.ExchangeRate = expense.Currency == "USD" ? exchange.USDA.Value : expense.Currency == "EUR" ? exchange.EURA.Value : 1;
            expense.CashID = cash.ID;
            expense.LocationID = location.LocationID;
            expense.OurCompanyID = location.OurCompanyID;
            expense.SlipDate = slipDatetime;
            expense.SlipNumber = slipnumber;
            expense.SlipPath = Server.MapPath("/");
            expense.TimeZone = location.Timezone.Value;
            expense.UID = Guid.NewGuid();
            expense.ResultID = dayresult.ID;
            expense.ExpenseTypeID = exptypeid;


            result = documentManager.AddCashExpense(expense, file, model.Authentication);

            model.DayResult = dayresult;
            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.CashActions = Db.VCashActions.Where(x => x.LocationID == dayresult.LocationID && x.ActionDate == dayresult.Date).ToList();
            model.Expenses = Db.VDocumentCashExpense.Where(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date && x.IsActive == true).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialExpence", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddExchange(long? id, long? itemid, HttpPostedFileBase file, string amount, string currency, string description, string exchange, string sysamount, string slipnumber, string slipdate, string sliptime)
        {
            Result<DocumentSaleExchange> result = new Result<DocumentSaleExchange>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 25);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var item = Db.DayResultItemList.FirstOrDefault(x => x.ID == itemid);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);

            var exchangeamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var exchangerate = Convert.ToDouble(exchange.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);


            //var expensequantity = 1;
            var exchangeitem = OfficeHelper.GetExchange(DateTime.UtcNow);
            var fromcash = OfficeHelper.GetCash(dayresult.LocationID, currency);
            var tocash = OfficeHelper.GetCash(dayresult.LocationID, "TRL");

            DateTime? slipDatetime = null;

            if (!string.IsNullOrEmpty(slipdate) && !string.IsNullOrEmpty(sliptime))
            {
                DateTime slipDate = Convert.ToDateTime(slipdate).Date;
                slipDatetime = slipDate.Add(Convert.ToDateTime(sliptime).TimeOfDay);
            }

            SaleExchange saleExchange = new SaleExchange();

            saleExchange.ActinTypeID = actType.ID;
            saleExchange.ActionTypeName = actType.Name;
            saleExchange.Amount = exchangeamount;
            saleExchange.Currency = currency;
            saleExchange.Description = description;
            saleExchange.DocumentDate = dayresult.Date;
            saleExchange.EnvironmentID = 2;
            saleExchange.ExchangeRate = saleExchange.Currency == "USD" ? exchangeitem.USDA.Value : saleExchange.Currency == "EUR" ? exchangeitem.EURA.Value : 1;
            saleExchange.FromCashID = fromcash.ID;
            saleExchange.ToCashID = tocash.ID;
            saleExchange.LocationID = location.LocationID;
            saleExchange.OurCompanyID = location.OurCompanyID;
            saleExchange.SlipDate = slipDatetime;
            saleExchange.SlipNumber = slipnumber;
            saleExchange.SlipPath = Server.MapPath("/");
            saleExchange.TimeZone = location.Timezone.Value;
            saleExchange.UID = Guid.NewGuid();
            saleExchange.ResultID = dayresult.ID;
            saleExchange.SaleExchangeRate = exchangerate;
            saleExchange.ToAmount = (saleExchange.Amount * saleExchange.SaleExchangeRate);
            saleExchange.ToCurrency = "TRL";


            result = documentManager.AddSaleExchange(saleExchange, file, model.Authentication);

            model.DayResult = dayresult;

            model.Exchanges = Db.VDocumentSaleExchange.Where(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date && x.IsActive == true).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialExchange", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddCollect(long? id, long? itemid, string amount, string currency, string description)
        {
            Result<DocumentCashCollections> result = new Result<DocumentCashCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 23);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);

            var paymentamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);

            CashCollection cashcollection = new CashCollection();

            cashcollection.ActinTypeID = actType.ID;
            cashcollection.ActinTypeName = actType.Name;
            cashcollection.Amount = paymentamount;
            cashcollection.Currency = currency;
            cashcollection.Description = description;
            cashcollection.DocumentDate = dayresult.Date;
            cashcollection.EnvironmentID = 2;
            cashcollection.ExchangeRate = cashcollection.Currency == "USD" ? exchange.USDA.Value : cashcollection.Currency == "EUR" ? exchange.EURA.Value : 1;
            cashcollection.LocationID = location.LocationID;
            cashcollection.UID = Guid.NewGuid();
            cashcollection.ResultID = dayresult.ID;
            cashcollection.FromCustomerID = Db.Customer.FirstOrDefault(x => x.OurCompanyID == location.OurCompanyID && x.IsActive == true)?.ID ?? null;

            result = documentManager.AddCashCollection(cashcollection, model.Authentication);

            model.DayResult = dayresult;

            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.CashActions = Db.VCashActions.Where(x => x.LocationID == dayresult.LocationID && x.ActionDate == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialCashCollectPayment", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddPayment(long? id, long? itemid, string amount, string currency, string description)
        {
            Result<DocumentCashPayments> result = new Result<DocumentCashPayments>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 27);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);

            var paymentamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);

            CashPayment cashpayment = new CashPayment();

            cashpayment.ActinTypeID = actType.ID;
            cashpayment.Amount = paymentamount;
            cashpayment.Currency = currency;
            cashpayment.Description = description;
            cashpayment.DocumentDate = dayresult.Date;
            cashpayment.EnvironmentID = 2;
            cashpayment.ExchangeRate = cashpayment.Currency == "USD" ? exchange.USDA.Value : cashpayment.Currency == "EUR" ? exchange.EURA.Value : 1;
            cashpayment.LocationID = location.LocationID;
            cashpayment.UID = Guid.NewGuid();
            cashpayment.ResultID = dayresult.ID;
            cashpayment.ToCustomerID = Db.Customer.FirstOrDefault(x => x.OurCompanyID == location.OurCompanyID && x.IsActive == true)?.ID ?? null;
            cashpayment.CashID = cash.ID;
            cashpayment.TimeZone = location.Timezone;
            cashpayment.OurCompanyID = location.OurCompanyID;
            cashpayment.ActionTypeName = actType.Name;

            result = documentManager.AddCashPayment(cashpayment, model.Authentication);

            model.DayResult = dayresult;

            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.CashActions = Db.VCashActions.Where(x => x.LocationID == dayresult.LocationID && x.ActionDate == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialCashCollectPayment", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddCardSale(long? id, string amount, string quantity, string currency, string description)
        {
            Result<DocumentPosCollections> result = new Result<DocumentPosCollections>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.BankActionType.FirstOrDefault(x => x.ID == 1);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == location.LocationID && x.IsActive == true && x.IsMaster == true);
            var bankaccount = Db.VBankAccount.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 2 && x.IsActive == true && x.IsMaster == true);

            var saleamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var salequantity = Convert.ToInt32(quantity);

            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);

            PosCollection poscollection = new PosCollection();

            poscollection.ActinTypeID = actType.ID;
            poscollection.ActionTypeName = actType.Name;
            poscollection.Amount = saleamount;
            poscollection.Quantity = salequantity;
            poscollection.Currency = currency;
            poscollection.Description = description;
            poscollection.DocumentDate = dayresult.Date;
            poscollection.EnvironmentID = 2;
            poscollection.ExchangeRate = poscollection.Currency == "USD" ? exchange.USDA.Value : poscollection.Currency == "EUR" ? exchange.EURA.Value : 1;
            poscollection.LocationID = location.LocationID;
            poscollection.UID = Guid.NewGuid();
            poscollection.ResultID = dayresult.ID;
            poscollection.FromCustomerID = Db.Customer.FirstOrDefault(x => x.OurCompanyID == location.OurCompanyID && x.IsActive == true)?.ID ?? null;
            poscollection.BankAccountID = bankaccount.ID;
            poscollection.TerminalID = posTerminal?.TerminalID.ToString() ?? "";
            poscollection.ResultID = dayresult.ID;
            poscollection.TimeZone = location.Timezone;
            poscollection.OurCompanyID = location.OurCompanyID;

            result = documentManager.AddPosCollection(poscollection, model.Authentication);

            model.DayResult = dayresult;

            model.BankActionTypes = Db.BankActionType.Where(x => x.IsActive == true).ToList();
            model.BankActions = Db.VBankActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();

            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialCardSale", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddCardRefund(long? id, string amount, string quantity, string currency, string description)
        {
            Result<DocumentPosCancel> result = new Result<DocumentPosCancel>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.BankActionType.FirstOrDefault(x => x.ID == 5);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            var posTerminal = Db.LocationPosTerminal.FirstOrDefault(x => x.LocationID == location.LocationID && x.IsActive == true && x.IsMaster == true);
            var bankaccount = Db.VBankAccount.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 2 && x.IsActive == true && x.IsMaster == true);

            var saleamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var salequantity = Convert.ToInt32(quantity.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);

            PosCancel poscancel = new PosCancel();

            poscancel.ActinTypeID = actType.ID;
            poscancel.ActionTypeName = actType.Name;
            poscancel.Amount = saleamount;
            poscancel.Quantity = salequantity;
            poscancel.Currency = currency;
            poscancel.Description = description;
            poscancel.DocumentDate = dayresult.Date;
            poscancel.EnvironmentID = 2;
            poscancel.ExchangeRate = poscancel.Currency == "USD" ? exchange.USDA.Value : poscancel.Currency == "EUR" ? exchange.EURA.Value : 1;
            poscancel.LocationID = location.LocationID;
            poscancel.UID = Guid.NewGuid();
            poscancel.ResultID = dayresult.ID;
            poscancel.FromBankAccountID = bankaccount.ID;
            poscancel.TerminalID = posTerminal?.TerminalID.ToString() ?? "";
            poscancel.ResultID = dayresult.ID;
            poscancel.TimeZone = location.Timezone;
            poscancel.OurCompanyID = location.OurCompanyID;


            result = documentManager.AddPosCancel(poscancel, model.Authentication);

            model.DayResult = dayresult;

            model.BankActionTypes = Db.BankActionType.Where(x => x.IsActive == true).ToList();
            model.BankActions = Db.VBankActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();

            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialCardSale", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddCashSale(long? id, string amount, string quantity, string currency, string description)
        {
            Result<DocumentTicketSales> result = new Result<DocumentTicketSales>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 24);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            
            var saleamount = Convert.ToDouble(amount.Replace(".", "").Replace(",","."),CultureInfo.InvariantCulture);
            var salequantity = Convert.ToInt32(quantity);

            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);

            CashSale cashsale = new CashSale();

            cashsale.ActinTypeID = actType.ID;
            cashsale.ActionTypeName = actType.Name;
            cashsale.Amount = saleamount;
            cashsale.Quantity = salequantity;
            cashsale.Currency = currency;
            cashsale.Description = description;
            cashsale.DocumentDate = dayresult.Date;
            cashsale.EnvironmentID = 2;
            cashsale.ExchangeRate = cashsale.Currency == "USD" ? exchange.USDA.Value : cashsale.Currency == "EUR" ? exchange.EURA.Value : 1;
            cashsale.LocationID = location.LocationID;
            cashsale.UID = Guid.NewGuid();
            cashsale.ResultID = dayresult.ID;
            cashsale.FromCustomerID = Db.Customer.FirstOrDefault(x => x.OurCompanyID == location.OurCompanyID && x.IsActive == true)?.ID ?? null;
            cashsale.TimeZone = location.Timezone;
            cashsale.OurCompanyID = location.OurCompanyID;
            cashsale.CashID = cash.ID;
            cashsale.PayMethodID = 1; // cash

            result = documentManager.AddCashSale(cashsale, model.Authentication);

            model.DayResult = dayresult;

            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.CashActions = Db.VCashActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();

            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialCashSale", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddCashSaleRefund(long? id, string amount, string quantity, string currency, string description)
        {
            Result<DocumentTicketSaleReturns> result = new Result<DocumentTicketSaleReturns>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

            DocumentManager documentManager = new DocumentManager();

            var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 28);
            var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == id);
            var location = Db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
            
            var saleamount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var salequantity = Convert.ToInt32(quantity);

            var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);
            var cash = OfficeHelper.GetCash(dayresult.LocationID, currency);

            SaleReturn salereturn = new SaleReturn();

            salereturn.ActinTypeID = actType.ID;
            salereturn.ActionTypeName = actType.Name;
            salereturn.Amount = saleamount;
            salereturn.Quantity = salequantity;
            salereturn.Currency = currency;
            salereturn.Description = description;
            salereturn.DocumentDate = dayresult.Date;
            salereturn.EnvironmentID = 2;
            salereturn.ExchangeRate = salereturn.Currency == "USD" ? exchange.USDA.Value : salereturn.Currency == "EUR" ? exchange.EURA.Value : 1;
            salereturn.LocationID = location.LocationID;
            salereturn.UID = Guid.NewGuid();
            salereturn.ResultID = dayresult.ID;
            salereturn.ToCustomerID = Db.Customer.FirstOrDefault(x => x.OurCompanyID == location.OurCompanyID && x.IsActive == true)?.ID ?? null;
            salereturn.TimeZone = location.Timezone;
            salereturn.OurCompanyID = location.OurCompanyID;
            salereturn.CashID = cash.ID;
            salereturn.PayMethodID = 1; // cash
            

            result = documentManager.AddCashSaleReturn(salereturn, model.Authentication);

            model.DayResult = dayresult;

            model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
            model.CashActions = Db.VCashActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();

            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            return PartialView("_PartialCashSale", model);
        }




    }
}