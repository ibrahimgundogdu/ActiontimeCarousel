using ActionForce.Entity;
using System;
using System.Collections.Generic;
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
                model.Exchanges = Db.VDocumentSaleExchange.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date).ToList();
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

                model.CashRecorderSlips = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date).ToList();
                model.DayResultDocuments = Db.VDayResultDocuments.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date).ToList();

                model.CurrencyList = OfficeHelper.GetCurrency();

                model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);

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
                var unitPrice = Convert.ToDouble(unithprice.Replace(".", ""));
                var totalAmount = Convert.ToDouble(totalamount.Replace(".", ""));

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
            var payamount = Convert.ToDouble(amount.Replace(".", ""));
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
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message};

            TempData["result"] = result;

            return PartialView("_PartialEmployeeSalary", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddBankTransfer(long? id, HttpPostedFileBase file, long? itemid,int bankid, string description, string slipnumber, string slipdate, string sliptime, string comission, string amount)
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
            var transamount = Convert.ToDouble(amount.Replace(".", ""));
            var transcommission = Convert.ToDouble(comission.Replace(".", ""));
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

            result = documentManager.AddBankTransfer(bankTransfer, file, model.Authentication);

            model.BankTransfers = Db.VDocumentBankTransfer.Where(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date).ToList();
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialBankTransfer", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddExpense(long? id,  long? itemid, HttpPostedFileBase file, int? exptypeid, string amount,string currency,string description, string slipnumber, string slipdate, string sliptime)
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
            var expenseamount = Convert.ToDouble(amount.Replace(".", ""));
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
            model.Result = new Result<DayResult>() { IsSuccess = result.IsSuccess, Message = result.Message };

            TempData["result"] = result;

            return PartialView("_PartialExpence", model);
        }






    }
}