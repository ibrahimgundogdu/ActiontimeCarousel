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
            model.BankAccountList = Db.BankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.SalaryEarn = Db.VDocumentSalaryEarn.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.SalaryEarn = model.SalaryEarn.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            }
            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

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

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddSalaryEarn(NewSalaryEarn cashSalary)
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
                var fromPrefix = cashSalary.EmployeeID;
                var amount = Convert.ToDouble(cashSalary.TotalAmount.Replace(".", ","));
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

                        DocumentSalaryEarn newCashColl = new DocumentSalaryEarn();

                        newCashColl.ActionTypeID = actType.ID;
                        newCashColl.ActionTypeName = actType.Name;
                        newCashColl.EmployeeID = fromPrefix;
                        newCashColl.TotalAmount = amount;
                        newCashColl.UnitPrice = (double?)cashSalary.UnitPrice;
                        newCashColl.QuantityHour = (double?)cashSalary.QuantityHour;
                        newCashColl.Currency = currency;
                        newCashColl.Date = docDate;
                        newCashColl.Description = cashSalary.Description;
                        newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "SE");
                        newCashColl.IsActive = true;
                        newCashColl.LocationID = cashSalary.LocationID;
                        newCashColl.OurCompanyID = location.OurCompanyID;
                        newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                        newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                        newCashColl.UID = Guid.NewGuid();

                        Db.DocumentSalaryEarn.Add(newCashColl);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        //OfficeHelper.AddCashAction(newCashColl.EmployeeID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.TotalAmount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);
                        OfficeHelper.AddEmployeeAction(newCashColl.EmployeeID, newCashColl.ActionTypeID, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.Description, 1, newCashColl.TotalAmount, 0, newCashColl.Currency, null, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş ödemesi başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", newCashColl.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);
                    }
                    else
                    {
                        result.Message = $"Kasa bakiyesi { amount } { currency } tutar için yeterli değildir. Kullanılabilir bakiye { balance } { currency } tutardır.";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }


                }
                catch (Exception ex)
                {

                    result.Message = $"Ücret Hakediş eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryEarn(NewSalaryEarn cashCollect)
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
                var fromPrefix = cashCollect.EmployeeID;
                var amount = Convert.ToDouble(cashCollect.TotalAmount.Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);

                var isDate = DateTime.Now.Date;
                var isEmp = cashCollect.EmployeeID;
                var isCash = Db.DocumentSalaryEarn.FirstOrDefault(x => x.UID == cashCollect.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);
                        isEmp = (int)isCash.EmployeeID;
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(docDate));

                        DocumentSalaryEarn self = new DocumentSalaryEarn()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            TotalAmount = isCash.TotalAmount,
                            EmployeeID = isCash.EmployeeID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            QuantityHour = isCash.QuantityHour,
                            ID = isCash.ID,
                            UnitPrice = isCash.UnitPrice,
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


                        isCash.EmployeeID = cashCollect.EmployeeID;
                        isCash.TotalAmount = amount;
                        isCash.UnitPrice = cashCollect.UnitPrice;
                        isCash.Description = cashCollect.Description;
                        isCash.QuantityHour = cashCollect.QuantityHour;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        //var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isCash.EmployeeID && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isCash.Date && x.DocumentNumber == isCash.DocumentNumber);

                        //if (cashaction != null)
                        //{
                        //    cashaction.Payment = isCash.TotalAmount;
                        //    cashaction.UpdateDate = isCash.UpdateDate;
                        //    cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                        //    Db.SaveChanges();

                        //}

                        var empaction = Db.EmployeeCashActions.FirstOrDefault(x => x.EmployeeID == isEmp && x.ActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate);

                        if (empaction != null)
                        {
                            empaction.Collection = isCash.TotalAmount;
                            empaction.Currency = cashCollect.Currency;
                            empaction.ProcessDate = docDate;
                            empaction.UpdateDate = isCash.UpdateDate;
                            empaction.UpdateEmployeeID = isCash.UpdateEmployee;

                            Db.SaveChanges();

                        }

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş başarı ile güncellendi";

                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSalaryEarn>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", isCash.ID.ToString(), "Salary", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Ücret Hakediş güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Update", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Detail", new { id = cashCollect.UID });
            //return RedirectToAction("Index", "Salary");

        }

        [AllowAnonymous]
        public ActionResult DeleteSalaryEarn(int? id)
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

                var isCash = Db.DocumentSalaryEarn.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        var exchange = OfficeHelper.GetExchange(Convert.ToDateTime(isCash.Date));

                        DocumentSalaryEarn self = new DocumentSalaryEarn()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            TotalAmount = isCash.TotalAmount,
                            EmployeeID = isCash.EmployeeID,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            Description = isCash.Description,
                            DocumentNumber = isCash.DocumentNumber,
                            QuantityHour = isCash.QuantityHour,
                            ID = isCash.ID,
                            UnitPrice = isCash.UnitPrice,
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


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddCashAction(isCash.EmployeeID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.TotalAmount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        //maaş hesap işlemi
                        OfficeHelper.AddEmployeeAction(isCash.EmployeeID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, -1 * isCash.TotalAmount, 0, isCash.Currency, null, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş iptal edildi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", isCash.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Ücret Hakediş iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", "-1", "Salary", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Index", "Salary");

        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
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
            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.Detail = Db.VDocumentSalaryEarn.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Salary" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.Detail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

        [AllowAnonymous]
        public string SalaryEarn(string id)
        {
            SalaryControlModel model = new SalaryControlModel();
            
            var fromPrefix = id.Substring(0, 1);
            var fromID = Convert.ToInt32(id.Substring(1, id.Length - 1));

            string dd = Db.EmployeeSalary.FirstOrDefault(x => x.EmployeeID == fromID).Hourly?.ToString();


            return dd;

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
            model.UnitPrice = Db.EmployeeSalary.ToList();
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
                        newCashColl.UID = Guid.NewGuid();

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

                        //OfficeHelper.AddCashAction(newCashColl.FromCashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

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
                                cashaction.UpdateDate = isCash.UpdateDate;
                                cashaction.EmployeeID = isCash.ToEmployeeID;
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
                                bankaction.UpdateDate = isCash.UpdateDate;
                                bankaction.EmployeeID = isCash.ToEmployeeID;
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
                            OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);
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
            return RedirectToAction("SalaryDetail", new { id = cashCollect.UID });
            //return RedirectToAction("SalaryPayment", "Salary");

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
                        OfficeHelper.AddCashAction(isCash.FromCashID, isCash.LocationID, isCash.ToEmployeeID, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

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
            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.SalaryDetail = Db.VDocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "Salary" && x.Action == "SalaryPayment" && x.Environment == "Office" && x.ProcessID == model.SalaryDetail.ID.ToString()).ToList();

            model.FromList = OfficeHelper.GetToList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Unit()  //
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<CashActions> ?? null;
            }

            model.UnitSalaryDistList = Db.VEmployeeSalaryDist.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.FullName).ToList();

            return View(model);
        }

        [AllowAnonymous]
        public PartialViewResult UnitSearch(string key, string active) //
        {
            SalaryControlModel model = new SalaryControlModel();

            model.UnitSalaryDistList = Db.VEmployeeSalaryDist.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();

            if (!string.IsNullOrEmpty(key))
            {
                key = key.ToUpper().Replace("İ", "I").Replace("Ü","U").Replace("Ğ", "G").Replace("Ş", "S").Replace("Ç", "C").Replace("Ö", "O");
                model.UnitSalaryDistList = model.UnitSalaryDistList.Where(x => x.FullNameSearch.Contains(key)).ToList();
            }

            if (!string.IsNullOrEmpty(active))
            {
                if (active == "act")
                {
                    model.UnitSalaryDistList = model.UnitSalaryDistList.Where(x => x.IsActive == true).ToList();
                }
                else if (active == "psv")
                {
                    model.UnitSalaryDistList = model.UnitSalaryDistList.Where(x => x.IsActive == false).ToList();
                }

            }

            return PartialView("_PartialUnitSalaryList", model);
        }
        
        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult SaveSalaryUnit(NewEmployeeSalary empSalary)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            SalaryControlModel model = new SalaryControlModel();

            if (empSalary != null)
            {
                var our = Db.VEmployeeSalary.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == our.OurCompanyID);
                var hourly = Convert.ToDouble(empSalary.Hourly.Replace(".", ","));
                var hourlyExtent = Convert.ToDouble(empSalary.HourlyExtend.Replace(".", ","));
                var extendMultiplyRate = Convert.ToDouble(empSalary.ExtendMultiplyRate.Replace(".", ","));

                var docDate = DateTime.Now.Date;

                if (DateTime.TryParse(empSalary.DateStart, out docDate))
                {
                    docDate = Convert.ToDateTime(empSalary.DateStart).Date;
                }

                var isSalary = Db.EmployeeSalary.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID && x.DateStart == docDate);

                if (isSalary == null)
                {
                    try
                    {
                        EmployeeSalary newEmpSalary = new EmployeeSalary();

                        newEmpSalary.EmployeeID = empSalary.EmployeeID;
                        newEmpSalary.DateStart = docDate;
                        newEmpSalary.Hourly = hourly;
                        newEmpSalary.Money = ourcompany.Currency;
                        newEmpSalary.HourlyExtend = hourlyExtent;
                        newEmpSalary.ExtendMultiplyRate = extendMultiplyRate;

                        Db.EmployeeSalary.Add(newEmpSalary);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = "Employee saatlik ücret başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", newEmpSalary.ID.ToString(), "Salary", "Unit", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newEmpSalary);
                    }
                    catch (Exception ex)
                    {
                        result.Message = $"Emplopyee saatlik ücret eklenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", "-1", "Salary", "Unit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"{our.FullName} Employee { isSalary.DateStart } tarihinde ücret girişi mevcuttur. Kontrol edip güncel tarihli ücret girişi yapabilirsiniz.";
                    
                }

                model.UnitSalaryList = Db.VEmployeeSalary.Where(x => x.EmployeeID == empSalary.EmployeeID).ToList();

            }


            TempData["result"] = result;

            model.Result = result;

            return PartialView("_PartialEmployeeSalaryList", model);
        }

        [AllowAnonymous]
        public PartialViewResult DeleteSalaryUnit(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            SalaryControlModel model = new SalaryControlModel();
            int? empID = 0;

            if (id != null)
            {
                
                var isCash = Db.EmployeeSalary.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {
                        empID = isCash.EmployeeID;
                        EmployeeSalary self = Db.EmployeeSalary.FirstOrDefault(x => x.ID == id);
                        self.ID = (int)id;

                        Db.EmployeeSalary.Remove(self);
                        Db.SaveChanges();

                        result.IsSuccess = true;
                        result.Message = $"{isCash.DateStart} tarihli {isCash.ID} ID'li Employee saatlik ücreti silindi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", isCash.ID.ToString(), "Salary", "Unit", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, isCash);

                    }
                    catch (Exception ex)
                    {
                        result.Message = $"{isCash.DateStart} tarihli {isCash.ID} ID'li Employee saatlik ücreti silinemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Remove", "-1", "Salary", "Unit", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                }
                model.UnitSalaryList = Db.VEmployeeSalary.Where(x => x.EmployeeID == empID).ToList();

            }

            TempData["result"] = result;

            model.Result = result;

            return PartialView("_PartialEmployeeSalaryList", model);

        }

        [AllowAnonymous]
        public PartialViewResult SalaryUnit(int? id)
        {
            SalaryControlModel model = new SalaryControlModel();
            
            model.UnitSalaryList = Db.VEmployeeSalary.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.EmployeeID == id).ToList();
            model.UnitSalary = model.UnitSalaryList.OrderByDescending(x=> x.DateStart).FirstOrDefault();
            return PartialView("_PartialAddEmployeeSalary", model);
        }
        
    }
}