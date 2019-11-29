using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class SalaryController : BaseController
    {
        // GET: Salary
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            SalaryControlModel model = new SalaryControlModel();

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

            model.SalaryEarn = Db.VDocumentSalaryEarn.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.SalaryEarn = model.SalaryEarn.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            

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

            return RedirectToAction("Index", "Salary");
        }



        [AllowAnonymous]
        public ActionResult SalaryPayment(int? locationId)
        {
            SalaryControlModel model = new SalaryControlModel();

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
        public ActionResult AddSalaryPayment(NewCashSalaryPayment cashSalary)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            SalaryControlModel model = new SalaryControlModel();

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
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", newCashColl.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
                catch (Exception ex)
                {

                    result.Message = $"Maaş Avans eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("SalaryPayment", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryPayment(NewCashSalaryPayment cashCollect)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            SalaryControlModel model = new SalaryControlModel();


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



                var isCash = Db.DocumentSalaryPayment.FirstOrDefault(x => x.UID == cashCollect.UID);
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
                        //isCash.SystemAmount = ourcompany.Currency == currency ? amount : amount * isCash.ExchangeRate;
                        //isCash.SystemCurrency = ourcompany.Currency;

                        Db.SaveChanges();

                        var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.FromCashID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);

                        if (cashaction != null)
                        {
                            cashaction.Payment = isCash.Amount;
                            cashaction.UpdateDate = isCash.UpdateDate;
                            cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        var empaction = Db.EmployeeCashActions.FirstOrDefault(x => x.EmployeeID == isCash.ToEmployeeID && x.ActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date);

                        if (empaction != null)
                        {
                            empaction.Collection = isCash.Amount;
                            empaction.UpdateDate = isCash.UpdateDate;
                            empaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSalaryPayment>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", isCash.ID.ToString(), "Salary", "SalaryPayment", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"Maaş Avans güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            //return RedirectToAction("CashDetail", new { id = cashCollect.UID });
            return RedirectToAction("SalaryPayment", "Salary");

        }

        [AllowAnonymous]
        public ActionResult DeleteSalaryPayment(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            SalaryControlModel model = new SalaryControlModel();


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
                        OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        //maaş hesap işlemi
                        OfficeHelper.AddEmployeeAction(isCash.ToEmployeeID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.SalaryType, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Maaş Avans ödemesi iptal edildi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", isCash.ID.ToString(), "Salary", "SalaryPayment", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Maaş Avans iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", "-1", "Salary", "SalaryPayment", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("SalaryPayment", "Salary");

        }

        [AllowAnonymous]
        public ActionResult SalaryDetail(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

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
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.SalaryDetail = Db.VDocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Salary" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.SalaryDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }
    }
}