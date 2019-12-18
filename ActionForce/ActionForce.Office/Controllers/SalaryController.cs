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
            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

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
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
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
                var fromPrefix = cashSalary.EmployeeID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashSalary.EmployeeID.Substring(1, cashSalary.EmployeeID.Length - 1));


                //var amount = Convert.ToDouble(cashSalary.TotalAmount.Replace(".", ","));
                var currency = cashSalary.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
                // tahsilat eklenir.

                SalaryEarn earn = new SalaryEarn();

                earn.ActionTypeID = actType.ID;
                earn.ActionTypeName = actType.Name;
                earn.Currency = cashSalary.Currency;
                earn.Description = cashSalary.Description;
                earn.DocumentDate = docDate;
                earn.EmployeeID = fromID;
                earn.EnvironmentID = 2;
                earn.LocationID = location.LocationID;
                earn.QuantityHour = (double)cashSalary.QuantityHour;
                earn.TotalAmount = (double)((double)cashSalary.QuantityHour * (double?)cashSalary.UnitPrice);
                earn.UID = Guid.NewGuid();
                earn.UnitPrice = (double?)cashSalary.UnitPrice;
                earn.TimeZone = location.Timezone;
                earn.OurCompanyID = location.OurCompanyID;



                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddSalaryEarn(earn, model.Authentication);

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

                var fromPrefix = cashCollect.EmployeeID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashCollect.EmployeeID.Substring(1, cashCollect.EmployeeID.Length - 1));

                //var amount = Convert.ToDouble(cashCollect.TotalAmount.Replace(".", ","));
                var currency = cashCollect.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashCollect.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashCollect.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash((int)cashCollect.LocationID, cashCollect.Currency);

                var isDate = DateTime.Now.Date;
                var isEmp = fromID;
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


                        isCash.EmployeeID = fromID;
                        isCash.TotalAmount = (double)((double?)cashCollect.UnitPrice * (double)cashCollect.QuantityHour);
                        isCash.UnitPrice = (double?)cashCollect.UnitPrice;
                        isCash.Description = cashCollect.Description;
                        isCash.QuantityHour = (double)cashCollect.QuantityHour;
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
                        OfficeHelper.AddEmployeeAction(isCash.EmployeeID, isCash.LocationID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, -1 * isCash.TotalAmount, 0, isCash.Currency, null, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

        [AllowAnonymous]
        public string SalaryEarn(string id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var fromPrefix = id.Substring(0, 1);
            var fromID = Convert.ToInt32(id.Substring(1, id.Length - 1));

            var isEmp = Db.EmployeeSalary.FirstOrDefault(x => x.EmployeeID == fromID);
            string dd = "";
            if (isEmp != null)
            {
                dd = Db.EmployeeSalary.FirstOrDefault(x => x.EmployeeID == fromID).Hourly?.ToString();
            }
            else
            {
                dd = "0";
            }
            
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

            return RedirectToAction("SalaryPayment", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddSalaryPayment(NewCashSalaryPayment cashSalary)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
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
                var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
                // tahsilat eklenir.
                var refID = string.IsNullOrEmpty(cashSalary.ReferanceID);
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
                payment.TimeZone = location.Timezone;
                payment.OurCompanyID = location.OurCompanyID;
                payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
                payment.FromBankID = (int?)cashSalary.BankAccountID > 0 ? cashSalary.BankAccountID : (int?)null;
                payment.FromCashID = (int?)cashSalary.BankAccountID == 0 ? cash.ID : (int?)null;
                payment.SalaryTypeID = cashSalary.SalaryType;
                payment.TimeZone = location.Timezone;
                payment.ReferanceID = refID == false ? Convert.ToInt64(cashSalary.ReferanceID) : (long?)null;


                DocumentManager documentManager = new DocumentManager();
                result = documentManager.AddSalaryPayment(payment, model.Authentication);
                

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
                            empaction.Payment = isCash.Amount;
                            empaction.Collection = 0;
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
                        OfficeHelper.AddEmployeeAction(isCash.ToEmployeeID, isCash.LocationID, isCash.ActionTypeID, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.Description, 1, -1 * isCash.Amount, 0, isCash.Currency, null, null, isCash.SalaryType, isCash.RecordEmployeeID, isCash.RecordDate);

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

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Unit()
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
                key = key.ToUpper().Replace("İ", "I").Replace("Ü", "U").Replace("Ğ", "G").Replace("Ş", "S").Replace("Ç", "C").Replace("Ö", "O");
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
                var our = Db.VEmployee.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID);
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

            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == id);

            model.UnitSalaryList = Db.VEmployeeSalary.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.EmployeeID == id).ToList();
            model.UnitSalary = model.UnitSalaryList.OrderByDescending(x => x.DateStart).FirstOrDefault();

            return PartialView("_PartialAddEmployeeSalary", model);
        }

        [AllowAnonymous]
        public ActionResult Current(int? employeeid)
        {
            SalaryControlModel model = new SalaryControlModel();


            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.EmployeeID = employeeid != null ? employeeid : Db.Employee.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).EmployeeID;
                filterModel.DateBegin = DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.Filters.EmployeeID);

            model.EmployeeActionList = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Filters.EmployeeID && x.ProcessDate >= model.Filters.DateBegin && x.ProcessDate <= model.Filters.DateEnd).OrderBy(x => x.ProcessDate).ToList();

            var balanceData = Db.VEmployeeCashActions.Where(x => x.EmployeeID == model.Filters.EmployeeID && x.ProcessDate < model.Filters.DateBegin).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalModel> headerTotals = new List<TotalModel>();


                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "TRL").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "USD").Sum(x => x.Amount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Salary",
                    Total = balanceData.Where(x => x.Currency == "EUR").Sum(x => x.Amount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Salary",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Salary",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Salary",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }




            List<TotalModel> footerTotals = new List<TotalModel>(); // ilk başta header ile footer aynı olur ekranda foreach içinde footer değişir. 

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "TRL").Total
            });



            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "USD").Total
            });



            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Salary",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Currency == "EUR").Total
            });





            model.FooterTotals = footerTotals;



            List<TotalModel> middleTotals = new List<TotalModel>();

            middleTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Salary",
                Total = 0
            });

            middleTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Salary",
                Total = 0
            });

            middleTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Salary",
                Total = 0
            });

            model.MiddleTotals = middleTotals;




            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterCurrent(int? employeeid, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.EmployeeID = employeeid;
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

            return RedirectToAction("Current", "Salary");
        }

    }
}