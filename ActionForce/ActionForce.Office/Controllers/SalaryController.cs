using ActionForce.Entity;
using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class SalaryController : BaseController
    {
        // GET: Salary
        [AllowAnonymous]
        public ActionResult Index(int? LocationID, int? EmployeeID, DateTime? DateBegin, DateTime? DateEnd)
        {
            SalaryControlModel model = new SalaryControlModel();

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

                filterModel.LocationID = LocationID ?? 0;
                filterModel.EmployeeID = EmployeeID ?? 0;
                filterModel.DateBegin = DateBegin != null ? DateBegin : DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.BankAccountList = Db.VBankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 1 && x.IsActive == true).ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.SalaryEarn = Db.VDocumentSalaryEarn.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            if (model.Filters.LocationID > 0)
            {
                model.SalaryEarn = model.SalaryEarn.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            if (model.Filters.EmployeeID > 0)
            {
                model.SalaryEarn = model.SalaryEarn.Where(x => x.EmployeeID == model.Filters.EmployeeID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AddNewSalaryEarn()
        {
            SalaryControlModel model = new SalaryControlModel();

            var actypes = new[] { 43, 44, 45, 46 };

            model.CashActionTypes = Db.CashActionType.Where(x => actypes.Contains(x.ID) && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();

            return View(model);
        }



        [HttpPost]
        [AllowAnonymous]
        public ActionResult Filter(int? LocationID, int? EmployeeID, DateTime? DateBegin, DateTime? DateEnd)
        {
            FilterModel model = new FilterModel();

            model.LocationID = LocationID;
            model.EmployeeID = EmployeeID;
            model.DateBegin = DateBegin;
            model.DateEnd = DateEnd;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (DateEnd == null)
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
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (cashSalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSalary.ActionTypeID);

                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);

                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
                var currency = cashSalary.Currency;
                var docDate = DateTime.UtcNow.AddHours(timezone).Date;

                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);

                // tahsilat eklenir.
                double? quantity = cashSalary.QuantityHour;
                double? price = Convert.ToDouble(cashSalary.Price.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                if (price != null && price > 0)
                {
                    SalaryEarn earn = new SalaryEarn();

                    earn.ActionTypeID = actType.ID;
                    earn.ActionTypeName = actType.Name;
                    earn.Currency = cashSalary.Currency;
                    earn.Description = cashSalary.Description;
                    earn.DocumentDate = docDate;
                    earn.EmployeeID = cashSalary.EmployeeID;
                    earn.EnvironmentID = 2;
                    earn.LocationID = location.LocationID;
                    earn.QuantityHour = quantity;
                    earn.TotalAmount = (double)(price);
                    earn.UID = Guid.NewGuid();
                    //earn.UnitPrice = (double?)cashSalary.UnitPrice;
                    earn.TimeZone = location.Timezone;
                    earn.OurCompanyID = location.OurCompanyID;
                    earn.CategoryID = cashSalary.CategoryID ?? (int?)null;
                    //earn.SystemQuantityHour = earn.QuantityHour;
                    //earn.SystemTotalAmount = earn.TotalAmount;
                    //earn.SystemUnitPrice = earn.UnitPrice;

                    var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == earn.LocationID && x.Date == earn.DocumentDate);

                    var salaryEarnEx = Db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == earn.LocationID && x.EmployeeID == earn.EmployeeID && x.Date == earn.DocumentDate && x.ActionTypeID == earn.ActionTypeID && x.TotalAmount == earn.TotalAmount);

                    if (salaryEarnEx == null)
                    {

                        DocumentSalaryEarn salaryEarn = new DocumentSalaryEarn();

                        salaryEarn.ActionTypeID = earn.ActionTypeID;
                        salaryEarn.ActionTypeName = earn.ActionTypeName;
                        salaryEarn.EmployeeID = earn.EmployeeID;
                        salaryEarn.QuantityHour = 0;
                        salaryEarn.UnitPrice = 0;
                        salaryEarn.TotalAmount = earn.TotalAmount;
                        salaryEarn.TotalAmountSalary = earn.TotalAmount;
                        salaryEarn.UnitPriceMultiplierApplied = 1;

                        salaryEarn.Currency = earn.Currency;
                        salaryEarn.Date = earn.DocumentDate;
                        salaryEarn.Description = earn.Description;
                        salaryEarn.DocumentNumber = OfficeHelper.GetDocumentNumber(earn.OurCompanyID, "SE");
                        salaryEarn.IsActive = true;
                        salaryEarn.LocationID = earn.LocationID;
                        salaryEarn.OurCompanyID = earn.OurCompanyID;
                        salaryEarn.RecordDate = DateTime.UtcNow.AddHours(earn.TimeZone.Value);
                        salaryEarn.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                        salaryEarn.RecordIP = OfficeHelper.GetIPAddress();
                        salaryEarn.UID = earn.UID;
                        salaryEarn.EnvironmentID = earn.EnvironmentID;
                        //salaryEarn.ReferenceID = salary.ReferanceID;
                        salaryEarn.ResultID = dayresult?.ID ?? 0;
                        salaryEarn.SystemQuantityHour = 0;
                        salaryEarn.SystemTotalAmount = earn.TotalAmount;
                        salaryEarn.SystemUnitPrice = earn.TotalAmount;
                        //salaryEarn.CategoryID = salary.CategoryID;

                        salaryEarn.UnitFoodPrice = 0;
                        salaryEarn.QuantityHourSalary = 1;
                        salaryEarn.QuantityHourFood = 0;

                        Db.DocumentSalaryEarn.Add(salaryEarn);
                        Db.SaveChanges();

                        // cari hesap işlemesi
                        OfficeHelper.AddEmployeeAction(salaryEarn.EmployeeID, salaryEarn.LocationID, salaryEarn.ActionTypeID, salaryEarn.ActionTypeName, salaryEarn.ID, salaryEarn.Date, salaryEarn.Description, 1, salaryEarn.TotalAmountSalary, 0, salaryEarn.Currency, null, null, null, salaryEarn.RecordEmployeeID, salaryEarn.RecordDate, salaryEarn.UID.Value, salaryEarn.DocumentNumber, 3);

                        result.IsSuccess = true;
                        result.Message = "Ücret Hakediş başarı ile eklendi";

                        // log atılır
                        OfficeHelper.AddApplicationLog("Office", "Salary", "Insert", salaryEarn.ID.ToString(), "Salary", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(earn.TimeZone.Value), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, salaryEarn);
                    }

                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

            }

            TempData["result"] = result;

            return RedirectToAction("Index", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryEarn(NewSalaryEarn cashEarn)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (cashEarn != null && cashEarn.ID > 0 && cashEarn.UID != null)
            {

                var salaryearn = Db.DocumentSalaryEarn.FirstOrDefault(x => x.ID == cashEarn.ID && x.UID == cashEarn.UID);


                if (salaryearn != null)
                {
                    salaryearn.Description = cashEarn.Description;
                    Db.SaveChanges();

                    var issuccess = OfficeHelper.CalculateSalaryEarn(salaryearn.ResultID.Value, salaryearn.EmployeeID.Value, salaryearn.Date.Value, salaryearn.LocationID.Value, model.Authentication);
                    result.IsSuccess = issuccess;
                    result.Message = "Hakediş başarı ile güncellendi";
                }
            }

            TempData["result"] = result;
            return RedirectToAction("Detail", new { id = cashEarn.UID });
        }

        [AllowAnonymous]
        public ActionResult DeleteSalaryEarn(string id)
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
                var delresult = documentManager.DeleteSalaryEarn(Guid.Parse(id), model.Authentication);

                result.Message = delresult.Message;
            }

            TempData["result"] = result;
            return RedirectToAction("Index");

        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 1 && x.IsActive == true).ToList();
            model.BankAccountList = Db.VBankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();

            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.Detail = Db.VDocumentSalaryEarn.FirstOrDefault(x => x.UID == id);
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Detail.LocationID);
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
        public ActionResult SalaryPayment(int? LocationID, int? EmployeeID, DateTime? DateBegin, DateTime? DateEnd)
        {
            SalaryControlModel model = new SalaryControlModel();

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

                filterModel.LocationID = LocationID ?? 0;
                filterModel.EmployeeID = EmployeeID ?? 0;
                filterModel.DateBegin = DateBegin != null ? DateBegin : DateTime.Now.AddMonths(-1).Date;
                filterModel.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.BankAccountList = Db.VBankAccount.ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.SalaryPayment = Db.VDocumentSalaryPayment.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

            if (model.Filters.LocationID > 0)
            {
                model.SalaryPayment = model.SalaryPayment.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            if (model.Filters.EmployeeID > 0)
            {
                model.SalaryPayment = model.SalaryPayment.Where(x => x.ToEmployeeID == model.Filters.EmployeeID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            }

            model.FromList = OfficeHelper.GetFromList(model.Authentication.ActionEmployee.OurCompanyID.Value).Where(x => x.Prefix == "E").ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterSalary(int? LocationID, int? EmployeeID, DateTime? DateBegin, DateTime? DateEnd)
        {
            FilterModel model = new FilterModel();

            model.LocationID = LocationID;
            model.EmployeeID = EmployeeID;
            model.DateBegin = DateBegin;
            model.DateEnd = DateEnd;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (DateEnd == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("SalaryPayment", "Salary");
        }

        [AllowAnonymous]
        public ActionResult NewSalaryPayment()
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 1 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.SalaryTypes = Db.SalaryType.Where(x => x.IsActive == true).ToList();

            return View(model);



        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddSalaryPayment(CashSalaryPayment cashsalary)
        {

            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashsalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 31); // maaş avans ödemesi

                if (cashsalary.CategoryID == 18)
                {
                    actType = Db.CashActionType.FirstOrDefault(x => x.ID == 38); // set card Ödemesi
                }

                else if (cashsalary.CategoryID == 11)
                {
                    actType = Db.CashActionType.FirstOrDefault(x => x.ID == 47); // Maaş Kesintisi
                }



                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashsalary.LocationID);
                var amount = Convert.ToDouble(cashsalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                int timezone = location.Timezone.Value;
                int? frombankID = null;
                int? fromcashID = null;

                if (cashsalary.FromBankID > 0)
                {
                    frombankID = cashsalary.FromBankID;
                }
                else
                {
                    var cash = OfficeHelper.GetCash(cashsalary.LocationID, cashsalary.Currency);
                    fromcashID = cash.ID;
                }

                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashsalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashsalary.DocumentDate).Date;
                }


                var exchange = OfficeHelper.GetExchange(docDate);



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
                    payment.FromBankID = frombankID;
                    payment.FromCashID = fromcashID;
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

            return RedirectToAction("SalaryPayment", "Salary");

            //Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            //{
            //    IsSuccess = false,
            //    Message = string.Empty,
            //    Data = null
            //};
            //SalaryControlModel model = new SalaryControlModel();

            //if (cashSalary != null)
            //{
            //    var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSalary.ActinTypeID);
            //    var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
            //    var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
            //    var fromPrefix = cashSalary.FromID.Substring(0, 1);
            //    var fromID = Convert.ToInt32(cashSalary.FromID.Substring(1, cashSalary.FromID.Length - 1));
            //    var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            //    var currency = cashSalary.Currency;
            //    var docDate = DateTime.Now.Date;
            //    int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;
            //    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

            //    if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
            //    {
            //        docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
            //    }
            //    var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
            //    // tahsilat eklenir.

            //    if (amount > 0)
            //    {
            //        SalaryPayment payment = new SalaryPayment();

            //        payment.ActinTypeID = actType.ID;
            //        payment.ActionTypeName = actType.Name;
            //        payment.Currency = cashSalary.Currency;
            //        payment.Description = cashSalary.Description;
            //        payment.DocumentDate = docDate;
            //        payment.EmployeeID = fromPrefix == "E" ? fromID : (int)0; ;
            //        payment.EnvironmentID = 2;
            //        payment.LocationID = location.LocationID;
            //        payment.Amount = amount;
            //        payment.UID = Guid.NewGuid();
            //        payment.TimeZone = location.Timezone;
            //        payment.OurCompanyID = location.OurCompanyID;
            //        payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
            //        payment.FromBankID = (int?)cashSalary.BankAccountID > 0 ? cashSalary.BankAccountID : (int?)null;
            //        payment.FromCashID = (int?)cashSalary.BankAccountID == 0 ? cash.ID : (int?)null;
            //        payment.SalaryTypeID = cashSalary.SalaryType;
            //        payment.TimeZone = location.Timezone;
            //        payment.ReferanceID = cashSalary.ReferanceID;
            //        payment.CategoryID = cashSalary.CategoryID ?? (int?)null;

            //        DocumentManager documentManager = new DocumentManager();
            //        result = documentManager.AddSalaryPayment(payment, model.Authentication);
            //    }
            //    else
            //    {
            //        result.IsSuccess = true;
            //        result.Message = $"Tutar 0'dan büyük olmalıdır.";
            //    }



            //}

            //Result<CashActions> messageresult = new Result<CashActions>();
            //messageresult.Message = result.Message;

            //TempData["result"] = messageresult;

            //return RedirectToAction("SalaryPayment", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryPayment(EditCashSalaryPayment cashSalary)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (cashSalary != null)
            {
                var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                double? newexchanges = Convert.ToDouble(cashSalary.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
                int timezone = location.Timezone != null ? location.Timezone.Value : location.Timezone.Value;
                bool isactive = !string.IsNullOrEmpty(cashSalary.IsActive) && cashSalary.IsActive == "1" ? true : false;

                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }

                int? frombankID = null;
                int? fromcashID = null;

                if (cashSalary.FromBankID > 0)
                {
                    frombankID = cashSalary.FromBankID;
                }
                else
                {
                    var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
                    fromcashID = cash.ID;
                }


                if (amount > 0)
                {
                    SalaryPayment salarypay = new SalaryPayment();

                    salarypay.LocationID = cashSalary.LocationID;
                    salarypay.Currency = cashSalary.Currency;
                    salarypay.DocumentDate = docDate;
                    salarypay.EmployeeID = cashSalary.EmployeeID;
                    salarypay.Amount = amount;
                    salarypay.Description = cashSalary.Description;
                    salarypay.SalaryTypeID = cashSalary.SalaryTypeID;
                    salarypay.UID = cashSalary.UID;
                    salarypay.CategoryID = cashSalary.CategoryID;
                    salarypay.TimeZone = timezone;
                    salarypay.ID = cashSalary.ID;
                    salarypay.ExchangeRate = newexchanges;
                    salarypay.FromBankID = frombankID;
                    salarypay.FromCashID = fromcashID;
                    salarypay.IsActive = isactive;
                    salarypay.Controller = "Cash";


                    if (newexchanges > 0)
                    {
                        salarypay.ExchangeRate = newexchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditSalaryPayment(salarypay, model.Authentication);

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

            //Result result = new Result()
            //{
            //    IsSuccess = false,
            //    Message = string.Empty
            //};

            //SalaryControlModel model = new SalaryControlModel();

            //if (cashSalary != null)
            //{
            //    var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashSalary.ActinTypeID);
            //    var location = Db.Location.FirstOrDefault(x => x.LocationID == cashSalary.LocationID);
            //    var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
            //    var fromPrefix = cashSalary.FromID.Substring(0, 1);
            //    var fromID = Convert.ToInt32(cashSalary.FromID.Substring(1, cashSalary.FromID.Length - 1));
            //    var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            //    var currency = cashSalary.Currency;
            //    var docDate = DateTime.Now.Date;
            //    int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

            //    if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
            //    {
            //        docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
            //    }
            //    double? newexchanges = Convert.ToDouble(cashSalary.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            //    double? exchanges = Convert.ToDouble(cashSalary.Exchange?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

            //    if (amount > 0)
            //    {
            //        SalaryPayment sale = new SalaryPayment();
            //        sale.LocationID = cashSalary.ActinTypeID;
            //        sale.Currency = cashSalary.Currency;
            //        sale.DocumentDate = docDate;
            //        sale.EmployeeID = fromPrefix == "E" ? fromID : (int)0;
            //        sale.Amount = amount;
            //        sale.Description = cashSalary.Description;
            //        sale.FromBankID = cashSalary.BankAccountID;
            //        sale.SalaryTypeID = cashSalary.SalaryType;
            //        sale.UID = cashSalary.UID;
            //        sale.CategoryID = cashSalary.CategoryID;
            //        sale.ReferanceID = cashSalary.ReferanceID;
            //        sale.TimeZone = timezone;
            //        if (newexchanges > 0)
            //        {
            //            sale.ExchangeRate = newexchanges;
            //        }
            //        else
            //        {
            //            sale.ExchangeRate = exchanges;
            //        }

            //        DocumentManager documentManager = new DocumentManager();
            //        var editresult = documentManager.EditSalaryPayment(sale, model.Authentication);

            //        result.IsSuccess = editresult.IsSuccess;
            //        result.Message = editresult.Message;

            //    }
            //    else
            //    {
            //        result.IsSuccess = true;
            //        result.Message = $"Tutar 0'dan büyük olmalıdır.";
            //    }



            //}

            //Result<CashActions> messageresult = new Result<CashActions>();
            //messageresult.Message = result.Message;

            //TempData["result"] = messageresult;
            //return RedirectToAction("SalaryDetail", new { id = cashSalary.UID });

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
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
            }

            model.SalaryTypes = Db.SalaryType.Where(x => x.IsActive == true).ToList();
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.AccountTypeID == 1 && x.IsActive == true).ToList();
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.SalaryDetail = Db.VDocumentSalaryPayment.FirstOrDefault(x => x.UID == id);
            //model.History = Db.ApplicationLog.Where(x => x.Controller == "Salary" && x.Action == "SalaryPayment" && x.Environment == "Office" && x.ProcessID == model.SalaryDetail.ID.ToString()).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.FullName).ToList();


            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Unit()
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result ?? null;
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
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (empSalary != null)
            {
                var our = Db.VEmployee.FirstOrDefault(x => x.EmployeeID == empSalary.EmployeeID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == our.OurCompanyID);

                var hourly = Convert.ToDouble(empSalary.Hourly.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var hourlyExtent = Convert.ToDouble(empSalary.HourlyExtend.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var extendMultiplyRate = Convert.ToDouble(empSalary.ExtendMultiplyRate.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

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
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
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

            var date = DateTime.UtcNow.Date;
            var prevdate = DateTime.UtcNow.AddDays(-60).Date; // 8 hafta öncesi

            var datebegin = Db.DateList.FirstOrDefault(x => x.DateKey == prevdate);
            var dateend = Db.DateList.FirstOrDefault(x => x.DateKey == date);


            model.DateList = Db.DateList.Where(x => x.DateKey >= prevdate && x.DateKey <= date).ToList();
            var firstday = model.DateList.OrderBy(x => x.DateKey).FirstOrDefault();

            model.ScheduleList = Db.VSchedule.Where(x => x.EmployeeID == id && x.ShiftDate >= firstday.DateKey).ToList();
            model.ShiftList = Db.VEmpShift.Where(x => x.EmployeeID == id && x.ShiftDate >= firstday.DateKey).ToList();

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

        [AllowAnonymous]
        public ActionResult Permit(int? employeeID)
        {
            SalaryControlModel model = new SalaryControlModel();

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

            if (employeeID != null && employeeID > 0)
            {
                model.Filters.EmployeeID = employeeID;
            }

            model.Permits = Db.VDocumentEmployeePermit.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.RecordDate).ToList();

            if (model.Filters.DateBegin != null && model.Filters.DateEnd != null)
            {
                model.Permits = model.Permits.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.RecordDate).ToList();
            }

            if (model.Filters.EmployeeID != null)
            {
                model.Permits = model.Permits.Where(x => x.EmployeeID == model.Filters.EmployeeID).OrderByDescending(x => x.RecordDate).ToList();
            }

            if (model.Filters.TypeID != null)
            {
                model.Permits = model.Permits.Where(x => x.PermitTypeID == model.Filters.TypeID).OrderByDescending(x => x.RecordDate).ToList();
            }

            model.PermitTypes = Db.PermitType.Where(x => x.IsActive == true);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PermitFilter(int? employeeID, int? permitType, DateTime? beginDate, DateTime? endDate)
        {
            FilterModel model = new FilterModel();

            model.EmployeeID = employeeID;
            model.DateBegin = beginDate;
            model.DateEnd = endDate;
            model.TypeID = permitType;

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

            return RedirectToAction("Permit", "Salary");
        }

        [AllowAnonymous]
        public ActionResult AddPermit()
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.InfoResult = TempData["result"] as Result ?? null;
            }

            model.PermitTypes = Db.PermitType.Where(x => x.IsActive == true);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).ToList();
            model.PermitStatus = Db.PermitStatus.Where(x => x.ID >= 0 && x.IsActive == true).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewPermit(NewPermit permit)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (permit != null)
            {
                var cashactType = Db.CashActionType.FirstOrDefault(x => x.ID == 36);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == permit.LocationID);
                var docDate = DateTime.Now.Date;
                var returnWorkDate = DateTime.Now.AddDays(1).Date;

                if (DateTime.TryParse(permit.Date, out docDate))
                {
                    docDate = Convert.ToDateTime(permit.Date).Date;
                }

                if (DateTime.TryParse(permit.ReturnWorkDate, out returnWorkDate))
                {
                    returnWorkDate = Convert.ToDateTime(permit.ReturnWorkDate).Date;
                }

                DateTime? beginDatetime = null;
                if (!string.IsNullOrEmpty(permit.DateBegin) && !string.IsNullOrEmpty(permit.DateBeginHour))
                {
                    DateTime slipDate = Convert.ToDateTime(permit.DateBegin).Date;
                    beginDatetime = slipDate.Add(Convert.ToDateTime(permit.DateBeginHour).TimeOfDay);
                }

                DateTime? endDatetime = null;
                if (!string.IsNullOrEmpty(permit.DateEnd) && !string.IsNullOrEmpty(permit.DateEndHour))
                {
                    DateTime slipDate = Convert.ToDateTime(permit.DateEnd).Date;
                    endDatetime = slipDate.Add(Convert.ToDateTime(permit.DateEndHour).TimeOfDay);
                }

                //var cash = OfficeHelper.GetCash(permit.LocationID, location.Currency);
                // tahsilat eklenir.

                if (beginDatetime != null && endDatetime != null && location != null)
                {
                    EmployeePermit permitdoc = new EmployeePermit();

                    permitdoc.ActinTypeID = cashactType.ID;
                    permitdoc.ActionTypeName = cashactType.Name;
                    permitdoc.Date = docDate;
                    permitdoc.DateBegin = beginDatetime.Value;
                    permitdoc.DateEnd = endDatetime.Value;
                    permitdoc.Description = permit.Description;
                    permitdoc.EmployeeID = permit.EmployeeID;
                    permitdoc.EnvironmentID = 2;
                    permitdoc.IsActive = true;
                    permitdoc.LocationID = location.LocationID;
                    permitdoc.OurCompanyID = location.OurCompanyID;
                    permitdoc.PermitTypeID = permit.PermitTypeID;
                    permitdoc.ReturnWorkDate = returnWorkDate;
                    permitdoc.StatusID = permit.StatusID;
                    permitdoc.TimeZone = location.Timezone.Value;
                    permitdoc.UID = Guid.NewGuid();


                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddEmployeePermit(permitdoc, model.Authentication);

                    TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

                    if (result.IsSuccess == true)
                    {
                        return RedirectToAction("PermitDetail", "Salary", new { id = permitdoc.UID });
                    }
                    else
                    {
                        return RedirectToAction("AddPermit", "Salary");
                    }
                }
                else
                {
                    result.Message = $"İzin başlangıç veya bitiş tarihlerinin her ikisi de dolu olmalıdır. Çalışan ve lokasyon seçilmelidir.";
                }
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            return RedirectToAction("AddPermit", "Salary");
        }

        [AllowAnonymous]
        public ActionResult PermitDetail(Guid id)
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["result"] != null)
            {
                model.InfoResult = TempData["result"] as Result ?? null;
            }

            model.CurrentPermit = Db.VDocumentEmployeePermit.FirstOrDefault(x => x.UID == id);
            model.PermitTypes = Db.PermitType.Where(x => x.IsActive == true);
            model.PermitStatus = Db.PermitStatus.Where(x => x.IsActive == true).ToList();
            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Permit" && x.ProcessID == model.CurrentPermit.ID.ToString()).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditPermit(EditPermit permit)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SalaryControlModel model = new SalaryControlModel();

            if (permit != null)
            {
                var cashactType = Db.CashActionType.FirstOrDefault(x => x.ID == permit.ActionTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == permit.LocationID);
                var docDate = DateTime.Now.Date;
                var returnWorkDate = DateTime.Now.AddDays(1).Date;
                bool isActive = !string.IsNullOrEmpty(permit.IsActive) && permit.IsActive == "1" ? true : false;

                if (DateTime.TryParse(permit.Date, out docDate))
                {
                    docDate = Convert.ToDateTime(permit.Date).Date;
                }

                if (DateTime.TryParse(permit.ReturnWorkDate, out returnWorkDate))
                {
                    returnWorkDate = Convert.ToDateTime(permit.ReturnWorkDate).Date;
                }

                DateTime? beginDatetime = null;
                if (!string.IsNullOrEmpty(permit.DateBegin) && !string.IsNullOrEmpty(permit.DateBeginHour))
                {
                    DateTime slipDate = Convert.ToDateTime(permit.DateBegin).Date;
                    beginDatetime = slipDate.Add(Convert.ToDateTime(permit.DateBeginHour).TimeOfDay);
                }

                DateTime? endDatetime = null;
                if (!string.IsNullOrEmpty(permit.DateEnd) && !string.IsNullOrEmpty(permit.DateEndHour))
                {
                    DateTime slipDate = Convert.ToDateTime(permit.DateEnd).Date;
                    endDatetime = slipDate.Add(Convert.ToDateTime(permit.DateEndHour).TimeOfDay);
                }

                // tahsilat eklenir.

                if (beginDatetime != null && endDatetime != null && location != null && endDatetime > beginDatetime)
                {
                    EmployeePermit permitdoc = new EmployeePermit();

                    permitdoc.ActinTypeID = cashactType.ID;
                    permitdoc.ActionTypeName = cashactType.Name;
                    permitdoc.Date = docDate;
                    permitdoc.DateBegin = beginDatetime.Value;
                    permitdoc.DateEnd = endDatetime.Value;
                    permitdoc.Description = permit.Description;
                    permitdoc.EmployeeID = permit.EmployeeID;
                    permitdoc.IsActive = isActive;
                    permitdoc.LocationID = location.LocationID;
                    permitdoc.OurCompanyID = location.OurCompanyID;
                    permitdoc.PermitTypeID = permit.PermitTypeID;
                    permitdoc.ReturnWorkDate = returnWorkDate;
                    permitdoc.StatusID = permit.StatusID;
                    permitdoc.TimeZone = location.Timezone.Value;
                    permitdoc.UID = permit.UID;
                    permitdoc.ID = permit.ID;

                    DocumentManager documentManager = new DocumentManager();
                    var editresult = documentManager.EditEmployeePermit(permitdoc, model.Authentication);


                    TempData["result"] = new Result() { IsSuccess = editresult.IsSuccess, Message = editresult.Message };

                    if (editresult.IsSuccess == true)
                    {
                        return RedirectToAction("PermitDetail", "Salary", new { id = permitdoc.UID });
                    }
                    else
                    {
                        return RedirectToAction("AddPermit", "Salary");
                    }
                }
                else
                {
                    if (endDatetime <= beginDatetime)
                    {
                        result.Message += $"İzin bitiş tarihi başlangış tarihden küçük olmamalıdır";
                    }

                    if (beginDatetime == null)
                    {
                        result.Message += $"İzin başlangıç tarihi boş olmamalıdır";
                    }

                    if (endDatetime == null)
                    {
                        result.Message += $"İzin bitiş tarihi boş olmamalıdır";
                    }

                    if (permit.LocationID <= 0 || permit.EmployeeID <= 0)
                    {
                        result.Message += $"Çalışan ve lokasyon mutlaka seçilmelidir.";
                    }
                }
            }
            else
            {
                result.Message = $"Form bilgileri gelmedi.";
            }

            TempData["result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };
            return RedirectToAction("AddPermit", "Salary");
        }

        //SalaryResult
        [AllowAnonymous]
        public ActionResult SalaryResult(int? EmployeeID, int? LocationID, int? SalaryPeriodID, DateTime? DateBegin, DateTime? DateEnd)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.EmployeeID = EmployeeID != null ? EmployeeID : 0;
                filterModel.LocationID = LocationID != null ? LocationID : 0;
                filterModel.SalaryPeriodID = SalaryPeriodID != null ? SalaryPeriodID : 0;
                filterModel.DateBegin = DateBegin != null ? DateBegin : DateTime.Now.Date;
                filterModel.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;
                model.Filters = filterModel;
            }

            if ((SalaryPeriodID != null && SalaryPeriodID > 0) || (model.Filters.SalaryPeriodID != null && model.Filters.SalaryPeriodID > 0))
            {
                model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == model.Filters.SalaryPeriodID);
                model.Filters.DateBegin = model.SalaryPeriod.DateBegin;
                model.Filters.DateEnd = model.SalaryPeriod.DateEnd;
            }

            model.SalaryPeriods = Db.VSalaryPeriod.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (model.Filters.EmployeeID > 0)
            {
                model.CurrentEmployee = Db.Employee.FirstOrDefault(x => x.EmployeeID == model.Filters.EmployeeID);
            }

            if (model.Filters.LocationID > 0)
            {
                model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            }

            var employeeCashActions = Db.VEmployeeCashActions.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ProcessDate >= model.Filters.DateBegin && x.ProcessDate <= model.Filters.DateEnd).OrderBy(x => x.ProcessDate);

            if (model.CurrentEmployee != null)
            {
                employeeCashActions = employeeCashActions.Where(x => x.EmployeeID == model.Filters.EmployeeID).OrderBy(x => x.ProcessDate);
            }

            if (model.CurrentLocation != null)
            {
                employeeCashActions = employeeCashActions.Where(x => x.LocationID == model.Filters.LocationID).OrderBy(x => x.ProcessDate);
            }

            model.EmployeeActionList = employeeCashActions.ToList();

            List<int> employeeids = model.EmployeeActionList.Select(x => x.EmployeeID.Value).Distinct().ToList();

            model.EmployeeModels = Db.VEmployeeModel.Where(x => employeeids.Contains(x.EmployeeID)).Select(x => new EmployeeModel()
            {
                EmployeeID = x.EmployeeID,
                EmployeeName = x.FullName,
                BankBranchCode = x.BankBranchCode,
                BankCode = x.BankCode,
                BankName = x.BankName,
                FoodCardNumber = x.FoodCardNumber,
                IBAN = x.IBAN,
                IdentityNumber = x.IdentityNumber,
                Currency = x.Currency,
                MobilePhone = x.MobilePhone,
                SalaryPaymentTypeID = x.SalaryPaymentTypeID ?? 1,
                LocationName = x.LocationName,
                SGKBranch = x.SGKBranch
            }).Distinct().ToList();

            var actiontypes = model.EmployeeActionList.Select(x => new { ID = x.ActionTypeID, Name = x.Name }).ToList();


            List<ResultTotalModel> footerTotals = new List<ResultTotalModel>();

            foreach (var item in actiontypes)
            {

                footerTotals.Add(new ResultTotalModel()
                {
                    Currency = "TRL",
                    ActionTypeID = item.ID.Value,
                    ActionTypeName = item.Name,
                    Total = model.EmployeeActionList.Where(x => x.ActionTypeID == item.ID && x.Currency == "TRL").Sum(x => x.Amount) ?? 0
                });

                footerTotals.Add(new ResultTotalModel()
                {
                    Currency = "USD",
                    ActionTypeID = item.ID.Value,
                    ActionTypeName = item.Name,
                    Total = model.EmployeeActionList.Where(x => x.ActionTypeID == item.ID && x.Currency == "USD").Sum(x => x.Amount) ?? 0
                });

                footerTotals.Add(new ResultTotalModel()
                {
                    Currency = "EUR",
                    ActionTypeID = item.ID.Value,
                    ActionTypeName = item.Name,
                    Total = model.EmployeeActionList.Where(x => x.ActionTypeID == item.ID && x.Currency == "EUR").Sum(x => x.Amount) ?? 0
                });

            }



            model.ResultFooterTotals = footerTotals;



            TempData["model"] = model;


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ResultFilter(int? EmployeeID, int? LocationID, int? SalaryPeriodID, DateTime? DateBegin, DateTime? DateEnd)
        {
            FilterModel model = new FilterModel();

            model.EmployeeID = EmployeeID;
            model.LocationID = LocationID;
            model.SalaryPeriodID = SalaryPeriodID;
            model.DateBegin = DateBegin;
            model.DateEnd = DateEnd;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.AddMonths(-1).Date;
                model.DateBegin = new DateTime(begin.Year, begin.Month, 1);
            }

            if (DateEnd == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("SalaryResult", "Salary");
        }

        [AllowAnonymous]
        public FileResult ExportData()
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var hakedisids = new[] { 32, 37, 43, 44, 45, 46 }.ToList(); // 39 set kart
            var odemeids = new[] { 31, 47, 36, 48, 49, 50, 51 }.ToList();  // 38 setcard

            string FileName = "SalaryResult_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string targetpath = Server.MapPath("~/Document/Salary/");
            string pathToExcelFile = targetpath + FileName;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("SalaryResult");

                    worksheet.Cell("A1").Value = "Adı";
                    worksheet.Cell("B1").Value = "TCKN";
                    worksheet.Cell("C1").Value = "Phone";
                    worksheet.Cell("D1").Value = "FoodCard";
                    worksheet.Cell("E1").Value = "IBAN";
                    worksheet.Cell("F1").Value = "Bank";
                    worksheet.Cell("G1").Value = "B";
                    worksheet.Cell("H1").Value = "SGKBranch";
                    worksheet.Cell("I1").Value = "LocationName";
                    worksheet.Cell("J1").Value = "Maaş";
                    worksheet.Cell("K1").Value = "İzin";
                    worksheet.Cell("L1").Value = "F.Mesai";
                    worksheet.Cell("M1").Value = "Prim";
                    worksheet.Cell("N1").Value = "Resmi";
                    worksheet.Cell("O1").Value = "Diğer";
                    worksheet.Cell("P1").Value = "Toplam";
                    worksheet.Cell("Q1").Value = "Avans Maaş";
                    worksheet.Cell("R1").Value = "Kesinti";
                    worksheet.Cell("S1").Value = "İzin";
                    worksheet.Cell("T1").Value = "F.Mesai";
                    worksheet.Cell("U1").Value = "Prim";
                    worksheet.Cell("V1").Value = "Resmi";
                    worksheet.Cell("W1").Value = "Diğer";
                    worksheet.Cell("X1").Value = "Toplam";
                    worksheet.Cell("Y1").Value = "Bakiye";
                    worksheet.Cell("Z1").Value = "SC Hakediş";
                    worksheet.Cell("AA1").Value = "SC Ödeme";
                    worksheet.Cell("AB1").Value = "SC Bakiye";
                    worksheet.Cell("AC1").Value = "Final";

                    //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                    int rownum = 2;

                    foreach (var emp in model.EmployeeModels.OrderBy(x => x.EmployeeName))
                    {
                        var maashakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 32 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var izinhakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 37 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var mesaihakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 43 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var primhakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 44 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var resmihakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 45 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var digerhakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 46 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var toplamhakedis = model.EmployeeActionList.Where(x => hakedisids.Contains(x.ActionTypeID.Value) && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;

                        var maas = model.EmployeeActionList.Where(x => x.ActionTypeID == 31 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var kesintip = model.EmployeeActionList.Where(x => x.ActionTypeID == 47 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var izinhakedisp = model.EmployeeActionList.Where(x => x.ActionTypeID == 36 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var mesaihakedisp = model.EmployeeActionList.Where(x => x.ActionTypeID == 48 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var primhakedisp = model.EmployeeActionList.Where(x => x.ActionTypeID == 49 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var resmihakedisp = model.EmployeeActionList.Where(x => x.ActionTypeID == 50 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var digerhakedisp = model.EmployeeActionList.Where(x => x.ActionTypeID == 51 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var toplamodeme = model.EmployeeActionList.Where(x => odemeids.Contains(x.ActionTypeID.Value) && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;

                        var setcardhakedis = model.EmployeeActionList.Where(x => x.ActionTypeID == 39 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;
                        var setcardodeme = model.EmployeeActionList.Where(x => x.ActionTypeID == 38 && x.EmployeeID == emp.EmployeeID).Sum(x => x.Amount) ?? 0;

                        var kumule = (toplamhakedis + toplamodeme + setcardodeme + setcardhakedis);


                        worksheet.Cell("A" + rownum).Value = emp.EmployeeName;
                        worksheet.Cell("B" + rownum).Value = emp.IdentityNumber;
                        worksheet.Cell("C" + rownum).Value = emp.MobilePhone;
                        worksheet.Cell("D" + rownum).Value = emp.FoodCardNumber;
                        worksheet.Cell("E" + rownum).Value = emp.IBAN;
                        worksheet.Cell("F" + rownum).Value = emp.BankName;
                        worksheet.Cell("G" + rownum).Value = emp.SalaryPaymentTypeID == 1 ? "B" : "";
                        worksheet.Cell("H" + rownum).Value = emp.SGKBranch;
                        worksheet.Cell("I" + rownum).Value = emp.LocationName;
                        worksheet.Cell("J" + rownum).Value = maashakedis;
                        worksheet.Cell("K" + rownum).Value = izinhakedis;
                        worksheet.Cell("L" + rownum).Value = mesaihakedis;
                        worksheet.Cell("M" + rownum).Value = primhakedis;
                        worksheet.Cell("N" + rownum).Value = resmihakedis;
                        worksheet.Cell("O" + rownum).Value = digerhakedis;
                        worksheet.Cell("P" + rownum).Value = toplamhakedis;
                        worksheet.Cell("Q" + rownum).Value = maas;
                        worksheet.Cell("R" + rownum).Value = kesintip;
                        worksheet.Cell("S" + rownum).Value = izinhakedisp;
                        worksheet.Cell("T" + rownum).Value = mesaihakedisp;
                        worksheet.Cell("U" + rownum).Value = primhakedisp;
                        worksheet.Cell("V" + rownum).Value = resmihakedisp;
                        worksheet.Cell("W" + rownum).Value = digerhakedisp;
                        worksheet.Cell("X" + rownum).Value = toplamodeme;
                        worksheet.Cell("Y" + rownum).Value = (toplamhakedis + toplamodeme);
                        worksheet.Cell("Z" + rownum).Value = setcardhakedis;
                        worksheet.Cell("AA" + rownum).Value = setcardodeme;
                        worksheet.Cell("AB" + rownum).Value = (setcardhakedis + setcardodeme);
                        worksheet.Cell("AC" + rownum).Value = kumule;

                        rownum++;
                    }

                    worksheet.Cell("A" + rownum).Value = null;
                    worksheet.Cell("B" + rownum).Value = "Toplam";
                    worksheet.Cell("C" + rownum).Value = null;
                    worksheet.Cell("D" + rownum).Value = null;
                    worksheet.Cell("E" + rownum).Value = null;
                    worksheet.Cell("F" + rownum).Value = null;
                    worksheet.Cell("G" + rownum).Value = null;
                    worksheet.Cell("H" + rownum).Value = null;
                    worksheet.Cell("I" + rownum).Value = null;
                    worksheet.Cell("J" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 32).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("K" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 37).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("L" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 43).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("M" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 44).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("N" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 45).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("O" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 46).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("P" + rownum).Value = model.EmployeeActionList.Where(x => hakedisids.Contains(x.ActionTypeID.Value)).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("Q" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 31).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("R" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 47).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("S" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 36).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("T" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 48).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("U" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 49).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("V" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 50).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("W" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 51).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("X" + rownum).Value = model.EmployeeActionList.Where(x => odemeids.Contains(x.ActionTypeID.Value)).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("Y" + rownum).Value = model.EmployeeActionList.Where(x => odemeids.Contains(x.ActionTypeID.Value) || hakedisids.Contains(x.ActionTypeID.Value)).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("Z" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 39).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("AA" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 38).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("AB" + rownum).Value = model.EmployeeActionList.Where(x => x.ActionTypeID == 38 || x.ActionTypeID == 39).Sum(x => x.Amount) ?? 0;
                    worksheet.Cell("AC" + rownum).Value = model.EmployeeActionList.Sum(x => x.Amount) ?? 0;

                    workbook.SaveAs(pathToExcelFile);
                }

            }
            catch (Exception ex)
            {
                return null;
            }

            return File(pathToExcelFile, "application/vnd.ms-excel", FileName);

        }

        [AllowAnonymous]
        public void ExportBankData()
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            Response.ClearContent();

            Response.ContentType = "application/force-download";
            Response.AddHeader("content-disposition",
                "attachment; filename=Bank_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            Response.Write("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
            Response.Write("<head>");
            Response.Write("<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            Response.Write("<!--[if gte mso 9]><xml>");
            Response.Write("<x:ExcelWorkbook>");
            Response.Write("<x:ExcelWorksheets>");
            Response.Write("<x:ExcelWorksheet>");
            Response.Write("<x:Name>Report Data</x:Name>");
            Response.Write("<x:WorksheetOptions>");
            Response.Write("<x:Print>");
            Response.Write("<x:ValidPrinterInfo/>");
            Response.Write("</x:Print>");
            Response.Write("</x:WorksheetOptions>");
            Response.Write("</x:ExcelWorksheet>");
            Response.Write("</x:ExcelWorksheets>");
            Response.Write("</x:ExcelWorkbook>");
            Response.Write("</xml>");
            Response.Write("<![endif]--> ");


            View("~/Views/Salary/ReportBankView.cshtml", model).ExecuteResult(this.ControllerContext);
            Response.Flush();
            Response.End();
        }

        [AllowAnonymous]
        public void ExportSetcardData()
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var existsids = model.EmployeeModels.Select(x => x.EmployeeID).ToList();
            var otherempids = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true && x.FoodCardNumber.Length > 0 && !existsids.Contains(x.EmployeeID)).Select(x => x.EmployeeID).Distinct().ToList();

            var _EmployeeModels = Db.VEmployeeModel.Where(x => otherempids.Contains(x.EmployeeID)).Select(x => new EmployeeModel()
            {
                EmployeeID = x.EmployeeID,
                EmployeeName = x.FullName,
                BankBranchCode = x.BankBranchCode,
                BankCode = x.BankCode,
                BankName = x.BankName,
                FoodCardNumber = x.FoodCardNumber,
                IBAN = x.IBAN,
                IdentityNumber = x.IdentityNumber,
                Currency = x.Currency,
                MobilePhone = x.MobilePhone
            }).Distinct().ToList();

            model.EmployeeModels.AddRange(_EmployeeModels);




            Response.ClearContent();

            Response.ContentType = "application/force-download";
            Response.AddHeader("content-disposition",
                "attachment; filename=Setcard_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
            Response.Write("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
            Response.Write("<head>");
            Response.Write("<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">");
            Response.Write("<!--[if gte mso 9]><xml>");
            Response.Write("<x:ExcelWorkbook>");
            Response.Write("<x:ExcelWorksheets>");
            Response.Write("<x:ExcelWorksheet>");
            Response.Write("<x:Name>Report Data</x:Name>");
            Response.Write("<x:WorksheetOptions>");
            Response.Write("<x:Print>");
            Response.Write("<x:ValidPrinterInfo/>");
            Response.Write("</x:Print>");
            Response.Write("</x:WorksheetOptions>");
            Response.Write("</x:ExcelWorksheet>");
            Response.Write("</x:ExcelWorksheets>");
            Response.Write("</x:ExcelWorkbook>");
            Response.Write("</xml>");
            Response.Write("<![endif]--> ");


            View("~/Views/Salary/ReportSetcardView.cshtml", model).ExecuteResult(this.ControllerContext);
            Response.Flush();
            Response.End();
        }

        [AllowAnonymous]
        public ActionResult NewSalaryPeriod(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }

            if (model.SalaryPeriod.GroupType == 1 && model.SalaryPeriod.SalaryPeriodGroupID == 1) // saat
            {
                Db.AddSalaryPeriodCompute(model.SalaryPeriod.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

            }

            if (model.SalaryPeriod.GroupType == 1 && model.SalaryPeriod.SalaryPeriodGroupID == 2) // aylık
            {
                Db.AddSalaryPeriodMonthCompute(model.SalaryPeriod.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

            }

            if (model.SalaryPeriod.GroupType == 2) // yemek kartı
            {
                Db.AddSalaryPeriodFoodCompute(model.SalaryPeriod.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
            }


            return RedirectToAction("DetailSalaryPeriod", new { id });
        }

        [AllowAnonymous]
        public ActionResult SalaryPeriods(int? GroupID, int? StatusID, int? Year)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.Year = Year;
                filterModel.GroupID = GroupID;
                filterModel.StatusID = StatusID;

                model.Filters = filterModel;
            }

            model.SalaryPeriods = Db.VSalaryPeriod.ToList();

            if (model.Filters.Year > 0)
            {
                model.SalaryPeriods = model.SalaryPeriods.Where(x => x.Year == model.Filters.Year).ToList();
            }

            if (model.Filters.GroupID > 0)
            {
                model.SalaryPeriods = model.SalaryPeriods.Where(x => x.SalaryPeriodGroupID == model.Filters.GroupID).ToList();
            }

            if (model.Filters.StatusID > 0)
            {
                model.SalaryPeriods = model.SalaryPeriods.Where(x => x.SalaryPeriodStatusID == model.Filters.StatusID).ToList();
            }

            model.Years = Db.VSalaryPeriod.Select(x => x.Year).Distinct().ToList();
            model.SalaryPeriodGroups = Db.SalaryPeriodGroup.ToList();
            model.SalaryPeriodStatus = Db.SalaryPeriodStatus.ToList();


            return View(model);
        }

        //FilterSalaryPeriods
        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterSalaryPeriods(int? GroupID, int? StatusID, int? Year)
        {
            FilterModel model = new FilterModel();

            model.GroupID = GroupID;
            model.StatusID = StatusID;
            model.Year = Year;

            TempData["filter"] = model;

            return RedirectToAction("SalaryPeriods", "Salary");
        }


        [AllowAnonymous]
        public ActionResult DetailSalaryPeriod(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == model.SalaryPeriod.ID).ToList();
            model.SalaryPeriodComputeSum = Db.VSalaryPeriodComputeSum.FirstOrDefault(x => x.SalaryPeriodID == model.SalaryPeriod.ID);

            if (model.SalaryPeriodComputeSum == null)
            {
                model.SalaryPeriodComputeSum = new VSalaryPeriodComputeSum();
            }

            if (Db.SalaryPeriodStatus.Any(x => x.ID == model.SalaryPeriod.SalaryPeriodStatusID + 1))
            {
                model.SalaryPeriodNextStatus = Db.SalaryPeriodStatus.FirstOrDefault(x => x.ID == model.SalaryPeriod.SalaryPeriodStatusID + 1);
            }
            else
            {
                model.SalaryPeriodNextStatus = Db.SalaryPeriodStatus.FirstOrDefault(x => x.ID == model.SalaryPeriod.SalaryPeriodStatusID);
            }

            TempData["model"] = model;

            return View(model);
        }

        //RemovePeriodCompute

        [AllowAnonymous]
        public ActionResult StatusSalaryPeriod(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var SalaryPeriod = Db.SalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }

            short statusid = SalaryPeriod.SalaryPeriodStatusID ?? 0;

            short newstatusid = (short)(statusid + 1);

            if (Db.SalaryPeriodStatus.Any(x => x.ID == newstatusid))
            {
                SalaryPeriod.SalaryPeriodStatusID = newstatusid;
                Db.SaveChanges();
            }

            if (SalaryPeriod.SalaryPeriodStatusID == 3)
            {
                Db.SetSalaryPeriodComputePayed(SalaryPeriod.ID);
            }

            OfficeHelper.AddApplicationLog("Office", "Salary", "Update", SalaryPeriod.ID.ToString(), "Salary", "StatusSalaryPeriod", null, true, $"{SalaryPeriod.ID} ID li periyodun durumu güncellendi", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, SalaryPeriod);

            return RedirectToAction("DetailSalaryPeriod", new { id });

        }

        [AllowAnonymous]
        public PartialViewResult SetEmployeeSalary(int? id, int? empid)
        {
            SalaryControlModel model = new SalaryControlModel();

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == id);
            model.SalaryPeriodCompute = Db.SalaryPeriodCompute.FirstOrDefault(x => x.SalaryPeriodID == id && x.EmployeeID == empid);


            return PartialView("_PartialSetEmployeeSalary", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryPaymentPeriod(FormSalaryPeriod perSalary)
        {
            SalaryControlModel model = new SalaryControlModel();
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (perSalary == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var salaryperiod = Db.SalaryPeriod.FirstOrDefault(x => x.ID == perSalary.SalaryPeriodID);
            var salaryComputed = Db.SalaryPeriodCompute.FirstOrDefault(x => x.ID == perSalary.ID && x.SalaryPeriodID == perSalary.SalaryPeriodID && x.EmployeeID == perSalary.EmployeeID);


            if (salaryComputed != null)
            {

                SalaryPeriodCompute self = new SalaryPeriodCompute()
                {
                    EmployeeID = salaryComputed.EmployeeID,
                    FullName = salaryComputed.FullName,
                    IdentityNumber = salaryComputed.IdentityNumber,
                    RecordDate = salaryComputed.RecordDate,
                    RecordEmployeeID = salaryComputed.RecordEmployeeID,
                    RecordIP = salaryComputed.RecordIP,
                    PremiumTotal = salaryComputed.PremiumTotal,
                    PhoneNumber = salaryComputed.PhoneNumber,
                    PermitTotal = salaryComputed.PermitTotal,
                    OtherTotal = salaryComputed.OtherTotal,
                    OtherPaymentAmount = salaryComputed.OtherPaymentAmount,
                    ManuelPaymentAmount = salaryComputed.ManuelPaymentAmount,
                    BankName = salaryComputed.BankName,
                    BankPaymentAmount = salaryComputed.BankPaymentAmount,
                    Currency = salaryComputed.Currency,
                    ExtraShiftTotal = salaryComputed.ExtraShiftTotal,
                    FoodCard = salaryComputed.FoodCard,
                    FoodCardPaymentAmount = salaryComputed.FoodCardPaymentAmount,
                    FoodCardTotal = salaryComputed.FoodCardTotal,
                    FormalTotal = salaryComputed.FormalTotal,
                    IBAN = salaryComputed.IBAN,
                    ID = salaryComputed.ID,
                    PrePaymentAmount = salaryComputed.PrePaymentAmount,
                    SalaryCutAmount = salaryComputed.SalaryCutAmount,
                    SalaryPeriodID = salaryComputed.SalaryPeriodID,
                    SalaryTotal = salaryComputed.SalaryTotal,
                    TotalBalance = salaryComputed.TotalBalance,
                    TotalPaymentAmount = salaryComputed.TotalPaymentAmount,
                    TotalProgress = salaryComputed.TotalProgress,
                    UID = salaryComputed.UID,
                    CostDate = salaryComputed.CostDate,
                    DV = salaryComputed.DV,
                    ExtraShiftPaymentAmount = salaryComputed.ExtraShiftPaymentAmount,
                    FoodcardBalance = salaryComputed.FoodcardBalance,
                    TotalCost = salaryComputed.TotalCost,
                    GV = salaryComputed.GV,
                    SSK = salaryComputed.SSK,
                    FormalPaymentAmount = salaryComputed.FormalPaymentAmount,
                    GrossBalance = salaryComputed.GrossBalance,
                    PermitPaymentAmount = salaryComputed.PermitPaymentAmount,
                    PremiumPaymentAmount = salaryComputed.PremiumPaymentAmount,
                    SalaryPaymentTypeID = salaryComputed.SalaryPaymentTypeID,
                    TransferBalance = salaryComputed.TransferBalance,
                    UpdateDate = salaryComputed.UpdateDate,
                    NetCost = salaryComputed.NetCost
                };

                var SalaryTotal = Convert.ToDouble((perSalary.SalaryTotal ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var PermitTotal = Convert.ToDouble((perSalary.PermitTotal ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var ExtraShiftTotal = Convert.ToDouble((perSalary.ExtraShiftTotal ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var PremiumTotal = Convert.ToDouble((perSalary.PremiumTotal ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var FormalTotal = Convert.ToDouble((perSalary.FormalTotal ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var OtherTotal = Convert.ToDouble((perSalary.OtherTotal ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var PrePaymentAmount = Convert.ToDouble((perSalary.PrePaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var SalaryCutAmount = Convert.ToDouble((perSalary.SalaryCutAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var PermitPaymentAmount = Convert.ToDouble((perSalary.PermitPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var ExtraShiftPaymentAmount = Convert.ToDouble((perSalary.ExtraShiftPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var PremiumPaymentAmount = Convert.ToDouble((perSalary.PremiumPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var FormalPaymentAmount = Convert.ToDouble((perSalary.FormalPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var OtherPaymentAmount = Convert.ToDouble((perSalary.OtherPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var BankPaymentAmount = Convert.ToDouble((perSalary.BankPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var ManuelPaymentAmount = Convert.ToDouble((perSalary.ManuelPaymentAmount ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var TransferBalance = Convert.ToDouble((perSalary.TransferBalance ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var NetCost = Convert.ToDouble((perSalary.NetCost ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var SSK = Convert.ToDouble((perSalary.SSK ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var GV = Convert.ToDouble((perSalary.GV ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var DV = Convert.ToDouble((perSalary.DV ?? "0").Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);




                salaryComputed.SalaryTotal = SalaryTotal;
                salaryComputed.PermitTotal = PermitTotal;
                salaryComputed.ExtraShiftTotal = ExtraShiftTotal;
                salaryComputed.PremiumTotal = PremiumTotal;
                salaryComputed.FormalTotal = FormalTotal;
                salaryComputed.OtherTotal = OtherTotal;

                salaryComputed.PrePaymentAmount = PrePaymentAmount * -1;
                salaryComputed.SalaryCutAmount = SalaryCutAmount * -1;
                salaryComputed.PermitPaymentAmount = PermitPaymentAmount * -1;
                salaryComputed.ExtraShiftPaymentAmount = ExtraShiftPaymentAmount * -1;
                salaryComputed.PremiumPaymentAmount = PremiumPaymentAmount * -1;
                salaryComputed.FormalPaymentAmount = FormalPaymentAmount * -1;
                salaryComputed.OtherPaymentAmount = OtherPaymentAmount * -1;


                salaryComputed.BankPaymentAmount = BankPaymentAmount * -1;
                salaryComputed.ManuelPaymentAmount = ManuelPaymentAmount * -1;
                salaryComputed.TransferBalance = TransferBalance;

                salaryComputed.NetCost = NetCost;
                salaryComputed.SSK = SSK;
                salaryComputed.GV = GV;
                salaryComputed.DV = DV;

                salaryComputed.RecordDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 3);
                salaryComputed.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                salaryComputed.RecordIP = OfficeHelper.GetIPAddress();

                Db.SaveChanges();

                Db.SalaryPeriodComputeSetTotal(salaryperiod.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

                result.IsSuccess = true;
                result.Message = $"{salaryComputed.FullName} adlı Çalışanın Maaş bilgisi başarı ile güncellendi";

                var isequal = OfficeHelper.PublicInstancePropertiesEqual<SalaryPeriodCompute>(self, salaryComputed, OfficeHelper.getIgnorelist());
                OfficeHelper.AddApplicationLog("Office", "Salary", "Update", salaryComputed.EmployeeID.ToString(), "SalaryPeriodCompute", "Detail", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);




            }
            else
            {
                result.IsSuccess = false;
                result.Message = $"{salaryComputed.FullName} adlı Çalışanın Maaş bilgisi bulunamadı";
            }

            TempData["result"] = result;
            return RedirectToAction("DetailSalaryPeriod", new { id = salaryperiod.UID });


        }


        [AllowAnonymous]
        public ActionResult RemovePeriodCompute(Guid? id, int? SalaryPeriodId)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }


            var SalaryPeriod = Db.SalaryPeriod.FirstOrDefault(x => x.ID == SalaryPeriodId);

            if (SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }

            // log alınmalı

            Db.RemoveSalaryPeriodCompute(id, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

            return RedirectToAction("DetailSalaryPeriod", new { id = SalaryPeriod.UID });


        }

        [AllowAnonymous]
        public FileResult ExportDataPeriod120(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            string FileName = "MaasPeriod120_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string targetpath = Server.MapPath("~/Document/Salary/");
            string pathToExcelFile = targetpath + FileName;

            var salaryPeriod = Db.SalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod.ID == salaryPeriod.ID)
            {

                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("MaasPeriod");

                        worksheet.Cell("A1").Value = "ID";
                        worksheet.Cell("B1").Value = "Adı";
                        worksheet.Cell("C1").Value = "TCKN";
                        worksheet.Cell("D1").Value = "Phone";
                        worksheet.Cell("E1").Value = "FoodCard";
                        worksheet.Cell("F1").Value = "IBAN";
                        worksheet.Cell("G1").Value = "Bank";
                        worksheet.Cell("H1").Value = "B";
                        worksheet.Cell("I1").Value = "SGKBranch";
                        worksheet.Cell("J1").Value = "LocationName";
                        worksheet.Cell("K1").Value = "Maaş";
                        worksheet.Cell("L1").Value = "İzin";
                        worksheet.Cell("M1").Value = "F.Mesai";
                        worksheet.Cell("N1").Value = "Prim";
                        worksheet.Cell("O1").Value = "Resmi";
                        worksheet.Cell("P1").Value = "Diğer";
                        worksheet.Cell("Q1").Value = "Toplam";
                        worksheet.Cell("R1").Value = "Avans Maaş";
                        worksheet.Cell("S1").Value = "Kesinti";
                        worksheet.Cell("T1").Value = "İzin";
                        worksheet.Cell("U1").Value = "F.Mesai";
                        worksheet.Cell("V1").Value = "Prim";
                        worksheet.Cell("W1").Value = "Resmi";
                        worksheet.Cell("X1").Value = "Diğer";
                        worksheet.Cell("Y1").Value = "Toplam";
                        worksheet.Cell("Z1").Value = "Bakiye";
                        worksheet.Cell("AA1").Value = "Bankadan";
                        worksheet.Cell("AB1").Value = "Elden";
                        worksheet.Cell("AC1").Value = "Devir";
                        worksheet.Cell("AD1").Value = "Final";
                        worksheet.Cell("AE1").Value = "Net Maliyet";
                        worksheet.Cell("AF1").Value = "SSK";
                        worksheet.Cell("AG1").Value = "GV";
                        worksheet.Cell("AH1").Value = "DV";
                        worksheet.Cell("AI1").Value = "Toplam Maliyet";
                        worksheet.Cell("AJ1").Value = "Güncellenme";

                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in model.SalaryPeriodComputes.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("B" + rownum).Value = item.FullName;
                            worksheet.Cell("C" + rownum).Value = item.IdentityNumber;
                            worksheet.Cell("D" + rownum).Value = item.PhoneNumber;
                            worksheet.Cell("E" + rownum).Value = item.FoodCard;
                            worksheet.Cell("F" + rownum).Value = item.IBAN;
                            worksheet.Cell("G" + rownum).Value = item.BankName;
                            worksheet.Cell("H" + rownum).Value = item.SalaryPaymentTypeID == 1 ? "B" : "";
                            worksheet.Cell("I" + rownum).Value = item.SGKBranch;
                            worksheet.Cell("J" + rownum).Value = item.LocationName;
                            worksheet.Cell("K" + rownum).Value = item.SalaryTotal;
                            worksheet.Cell("L" + rownum).Value = item.PermitTotal;
                            worksheet.Cell("M" + rownum).Value = item.ExtraShiftTotal;
                            worksheet.Cell("N" + rownum).Value = item.PremiumTotal;
                            worksheet.Cell("O" + rownum).Value = item.FormalTotal;
                            worksheet.Cell("P" + rownum).Value = item.OtherTotal;
                            worksheet.Cell("Q" + rownum).Value = item.TotalProgress;

                            worksheet.Cell("R" + rownum).Value = item.PrePaymentAmount;
                            worksheet.Cell("S" + rownum).Value = item.SalaryCutAmount;
                            worksheet.Cell("T" + rownum).Value = item.PermitPaymentAmount;
                            worksheet.Cell("U" + rownum).Value = item.ExtraShiftPaymentAmount;
                            worksheet.Cell("V" + rownum).Value = item.PremiumPaymentAmount;
                            worksheet.Cell("W" + rownum).Value = item.FormalPaymentAmount;
                            worksheet.Cell("X" + rownum).Value = item.OtherPaymentAmount;
                            worksheet.Cell("Y" + rownum).Value = item.TotalPaymentAmount;
                            worksheet.Cell("Z" + rownum).Value = item.TotalBalance;

                            worksheet.Cell("AA" + rownum).Value = item.BankPaymentAmount;
                            worksheet.Cell("AB" + rownum).Value = item.ManuelPaymentAmount;
                            worksheet.Cell("AC" + rownum).Value = item.TransferBalance;
                            worksheet.Cell("AD" + rownum).Value = item.GrossBalance;

                            worksheet.Cell("AE" + rownum).Value = item.NetCost;
                            worksheet.Cell("AF" + rownum).Value = item.SSK;
                            worksheet.Cell("AG" + rownum).Value = item.GV;
                            worksheet.Cell("AH" + rownum).Value = item.DV;
                            worksheet.Cell("AI" + rownum).Value = item.TotalCost;

                            worksheet.Cell("AJ" + rownum).Value = item.UpdateDate;

                            rownum++;
                        }

                        worksheet.Cell("A" + rownum).Value = null;
                        worksheet.Cell("B" + rownum).Value = null;
                        worksheet.Cell("C" + rownum).Value = null;
                        worksheet.Cell("D" + rownum).Value = null;
                        worksheet.Cell("E" + rownum).Value = null;
                        worksheet.Cell("F" + rownum).Value = null;
                        worksheet.Cell("G" + rownum).Value = "Toplam";
                        worksheet.Cell("H" + rownum).Value = null;
                        worksheet.Cell("I" + rownum).Value = null;
                        worksheet.Cell("J" + rownum).Value = null;
                        worksheet.Cell("K" + rownum).Value = model.SalaryPeriodComputeSum.SalaryTotal;
                        worksheet.Cell("L" + rownum).Value = model.SalaryPeriodComputeSum.PermitTotal;
                        worksheet.Cell("M" + rownum).Value = model.SalaryPeriodComputeSum.ExtraShiftTotal;
                        worksheet.Cell("N" + rownum).Value = model.SalaryPeriodComputeSum.PremiumTotal;
                        worksheet.Cell("O" + rownum).Value = model.SalaryPeriodComputeSum.FormalTotal;
                        worksheet.Cell("P" + rownum).Value = model.SalaryPeriodComputeSum.OtherTotal;
                        worksheet.Cell("Q" + rownum).Value = model.SalaryPeriodComputeSum.TotalProgress;

                        worksheet.Cell("R" + rownum).Value = model.SalaryPeriodComputeSum.PrePaymentAmount;
                        worksheet.Cell("S" + rownum).Value = model.SalaryPeriodComputeSum.SalaryCutAmount;
                        worksheet.Cell("T" + rownum).Value = model.SalaryPeriodComputeSum.PermitPaymentAmount;
                        worksheet.Cell("U" + rownum).Value = model.SalaryPeriodComputeSum.ExtraShiftPaymentAmount;
                        worksheet.Cell("V" + rownum).Value = model.SalaryPeriodComputeSum.PremiumPaymentAmount;
                        worksheet.Cell("W" + rownum).Value = model.SalaryPeriodComputeSum.FormalPaymentAmount;
                        worksheet.Cell("X" + rownum).Value = model.SalaryPeriodComputeSum.OtherPaymentAmount;
                        worksheet.Cell("Y" + rownum).Value = model.SalaryPeriodComputeSum.TotalPaymentAmount;
                        worksheet.Cell("Z" + rownum).Value = model.SalaryPeriodComputeSum.TotalBalance;

                        worksheet.Cell("AA" + rownum).Value = model.SalaryPeriodComputeSum.BankPaymentAmount;
                        worksheet.Cell("AB" + rownum).Value = model.SalaryPeriodComputeSum.ManuelPaymentAmount;
                        worksheet.Cell("AC" + rownum).Value = model.SalaryPeriodComputeSum.TransferBalance;
                        worksheet.Cell("AD" + rownum).Value = model.SalaryPeriodComputeSum.GrossBalance;

                        worksheet.Cell("AE" + rownum).Value = model.SalaryPeriodComputeSum.NetCost;
                        worksheet.Cell("AF" + rownum).Value = model.SalaryPeriodComputeSum.SSK;
                        worksheet.Cell("AG" + rownum).Value = model.SalaryPeriodComputeSum.GV;
                        worksheet.Cell("AH" + rownum).Value = model.SalaryPeriodComputeSum.DV;
                        worksheet.Cell("AI" + rownum).Value = model.SalaryPeriodComputeSum.TotalCost;

                        worksheet.Cell("AJ" + rownum).Value = null;


                        workbook.SaveAs(pathToExcelFile);
                    }

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return File(pathToExcelFile, "application/vnd.ms-excel", FileName);
        }

        [AllowAnonymous]
        public FileResult ExportDataPeriod110(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            string FileName = "SaatPeriod110_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string targetpath = Server.MapPath("~/Document/Salary/");
            string pathToExcelFile = targetpath + FileName;
            var salaryPeriod = Db.SalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod.ID == salaryPeriod.ID)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("SaatPeriod");

                        worksheet.Cell("A1").Value = "ID";
                        worksheet.Cell("B1").Value = "Adı";
                        worksheet.Cell("C1").Value = "TCKN";
                        worksheet.Cell("D1").Value = "Phone";
                        worksheet.Cell("E1").Value = "FoodCard";
                        worksheet.Cell("F1").Value = "IBAN";
                        worksheet.Cell("G1").Value = "Bank";
                        worksheet.Cell("H1").Value = "B";
                        worksheet.Cell("I1").Value = "SGKBranch";
                        worksheet.Cell("J1").Value = "LocationName";
                        worksheet.Cell("K1").Value = "Maaş";
                        worksheet.Cell("L1").Value = "İzin";
                        worksheet.Cell("M1").Value = "F.Mesai";
                        worksheet.Cell("N1").Value = "Prim";
                        worksheet.Cell("O1").Value = "Resmi";
                        worksheet.Cell("P1").Value = "Diğer";
                        worksheet.Cell("Q1").Value = "Toplam";
                        worksheet.Cell("R1").Value = "Avans Maaş";
                        worksheet.Cell("S1").Value = "Kesinti";
                        worksheet.Cell("T1").Value = "İzin";
                        worksheet.Cell("U1").Value = "F.Mesai";
                        worksheet.Cell("V1").Value = "Prim";
                        worksheet.Cell("W1").Value = "Resmi";
                        worksheet.Cell("X1").Value = "Diğer";
                        worksheet.Cell("Y1").Value = "Toplam";
                        worksheet.Cell("Z1").Value = "Bakiye";
                        worksheet.Cell("AA1").Value = "Bankadan";
                        worksheet.Cell("AB1").Value = "Elden";
                        worksheet.Cell("AC1").Value = "Devir";
                        worksheet.Cell("AD1").Value = "Final";
                        worksheet.Cell("AE1").Value = "Net Maliyet";
                        worksheet.Cell("AF1").Value = "SSK";
                        worksheet.Cell("AG1").Value = "GV";
                        worksheet.Cell("AH1").Value = "DV";
                        worksheet.Cell("AI1").Value = "Toplam Maliyet";
                        worksheet.Cell("AJ1").Value = "Güncellenme";

                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in model.SalaryPeriodComputes.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("B" + rownum).Value = item.FullName;
                            worksheet.Cell("C" + rownum).Value = item.IdentityNumber;
                            worksheet.Cell("D" + rownum).Value = item.PhoneNumber;
                            worksheet.Cell("E" + rownum).Value = item.FoodCard;
                            worksheet.Cell("F" + rownum).Value = item.IBAN;
                            worksheet.Cell("G" + rownum).Value = item.BankName;
                            worksheet.Cell("H" + rownum).Value = item.SalaryPaymentTypeID == 1 ? "B" : "";
                            worksheet.Cell("I" + rownum).Value = item.SGKBranch;
                            worksheet.Cell("J" + rownum).Value = item.LocationName;
                            worksheet.Cell("K" + rownum).Value = item.SalaryTotal;
                            worksheet.Cell("L" + rownum).Value = item.PermitTotal;
                            worksheet.Cell("M" + rownum).Value = item.ExtraShiftTotal;
                            worksheet.Cell("N" + rownum).Value = item.PremiumTotal;
                            worksheet.Cell("O" + rownum).Value = item.FormalTotal;
                            worksheet.Cell("P" + rownum).Value = item.OtherTotal;
                            worksheet.Cell("Q" + rownum).Value = item.TotalProgress;

                            worksheet.Cell("R" + rownum).Value = item.PrePaymentAmount;
                            worksheet.Cell("S" + rownum).Value = item.SalaryCutAmount;
                            worksheet.Cell("T" + rownum).Value = item.PermitPaymentAmount;
                            worksheet.Cell("U" + rownum).Value = item.ExtraShiftPaymentAmount;
                            worksheet.Cell("V" + rownum).Value = item.PremiumPaymentAmount;
                            worksheet.Cell("W" + rownum).Value = item.FormalPaymentAmount;
                            worksheet.Cell("X" + rownum).Value = item.OtherPaymentAmount;
                            worksheet.Cell("Y" + rownum).Value = item.TotalPaymentAmount;
                            worksheet.Cell("Z" + rownum).Value = item.TotalBalance;

                            worksheet.Cell("AA" + rownum).Value = item.BankPaymentAmount;
                            worksheet.Cell("AB" + rownum).Value = item.ManuelPaymentAmount;
                            worksheet.Cell("AC" + rownum).Value = item.TransferBalance;
                            worksheet.Cell("AD" + rownum).Value = item.GrossBalance;

                            worksheet.Cell("AE" + rownum).Value = item.NetCost;
                            worksheet.Cell("AF" + rownum).Value = item.SSK;
                            worksheet.Cell("AG" + rownum).Value = item.GV;
                            worksheet.Cell("AH" + rownum).Value = item.DV;
                            worksheet.Cell("AI" + rownum).Value = item.TotalCost;

                            worksheet.Cell("AJ" + rownum).Value = item.UpdateDate;

                            rownum++;
                        }

                        worksheet.Cell("A" + rownum).Value = null;
                        worksheet.Cell("B" + rownum).Value = null;
                        worksheet.Cell("C" + rownum).Value = null;
                        worksheet.Cell("D" + rownum).Value = null;
                        worksheet.Cell("E" + rownum).Value = null;
                        worksheet.Cell("F" + rownum).Value = null;
                        worksheet.Cell("G" + rownum).Value = "Toplam";
                        worksheet.Cell("H" + rownum).Value = null;
                        worksheet.Cell("I" + rownum).Value = null;
                        worksheet.Cell("J" + rownum).Value = null;
                        worksheet.Cell("K" + rownum).Value = model.SalaryPeriodComputeSum.SalaryTotal;
                        worksheet.Cell("L" + rownum).Value = model.SalaryPeriodComputeSum.PermitTotal;
                        worksheet.Cell("M" + rownum).Value = model.SalaryPeriodComputeSum.ExtraShiftTotal;
                        worksheet.Cell("N" + rownum).Value = model.SalaryPeriodComputeSum.PremiumTotal;
                        worksheet.Cell("O" + rownum).Value = model.SalaryPeriodComputeSum.FormalTotal;
                        worksheet.Cell("P" + rownum).Value = model.SalaryPeriodComputeSum.OtherTotal;
                        worksheet.Cell("Q" + rownum).Value = model.SalaryPeriodComputeSum.TotalProgress;

                        worksheet.Cell("R" + rownum).Value = model.SalaryPeriodComputeSum.PrePaymentAmount;
                        worksheet.Cell("S" + rownum).Value = model.SalaryPeriodComputeSum.SalaryCutAmount;
                        worksheet.Cell("T" + rownum).Value = model.SalaryPeriodComputeSum.PermitPaymentAmount;
                        worksheet.Cell("U" + rownum).Value = model.SalaryPeriodComputeSum.ExtraShiftPaymentAmount;
                        worksheet.Cell("V" + rownum).Value = model.SalaryPeriodComputeSum.PremiumPaymentAmount;
                        worksheet.Cell("W" + rownum).Value = model.SalaryPeriodComputeSum.FormalPaymentAmount;
                        worksheet.Cell("X" + rownum).Value = model.SalaryPeriodComputeSum.OtherPaymentAmount;
                        worksheet.Cell("Y" + rownum).Value = model.SalaryPeriodComputeSum.TotalPaymentAmount;
                        worksheet.Cell("Z" + rownum).Value = model.SalaryPeriodComputeSum.TotalBalance;

                        worksheet.Cell("AA" + rownum).Value = model.SalaryPeriodComputeSum.BankPaymentAmount;
                        worksheet.Cell("AB" + rownum).Value = model.SalaryPeriodComputeSum.ManuelPaymentAmount;
                        worksheet.Cell("AC" + rownum).Value = model.SalaryPeriodComputeSum.TransferBalance;
                        worksheet.Cell("AD" + rownum).Value = model.SalaryPeriodComputeSum.GrossBalance;

                        worksheet.Cell("AE" + rownum).Value = model.SalaryPeriodComputeSum.NetCost;
                        worksheet.Cell("AF" + rownum).Value = model.SalaryPeriodComputeSum.SSK;
                        worksheet.Cell("AG" + rownum).Value = model.SalaryPeriodComputeSum.GV;
                        worksheet.Cell("AH" + rownum).Value = model.SalaryPeriodComputeSum.DV;
                        worksheet.Cell("AI" + rownum).Value = model.SalaryPeriodComputeSum.TotalCost;

                        worksheet.Cell("AJ" + rownum).Value = null;



                        workbook.SaveAs(pathToExcelFile);
                    }

                }
                catch (Exception ex)
                {
                    return null;
                }

            }
            else
            {
                return null;
            }

            return File(pathToExcelFile, "application/vnd.ms-excel", FileName);
        }

        [AllowAnonymous]
        public FileResult ExportDataPeriod210(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;


            string FileName = "FoodCardPeriod210_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string targetpath = Server.MapPath("~/Document/Salary/");
            string pathToExcelFile = targetpath + FileName;
            var salaryPeriod = Db.SalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod.ID == salaryPeriod.ID)
            {

                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("FoodCardPeriod");

                        worksheet.Cell("A1").Value = "ID";
                        worksheet.Cell("B1").Value = "Adı";
                        worksheet.Cell("C1").Value = "TCKN";
                        worksheet.Cell("D1").Value = "Phone";
                        worksheet.Cell("E1").Value = "FoodCard";
                        worksheet.Cell("F1").Value = "IBAN";
                        worksheet.Cell("G1").Value = "Bank";
                        worksheet.Cell("H1").Value = "Hakediş";
                        worksheet.Cell("I1").Value = "Ödeme";
                        worksheet.Cell("J1").Value = "Bakiye";
                        worksheet.Cell("K1").Value = "Güncellenme";

                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in model.SalaryPeriodComputes.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("B" + rownum).Value = item.FullName;
                            worksheet.Cell("C" + rownum).Value = item.IdentityNumber;
                            worksheet.Cell("D" + rownum).Value = item.PhoneNumber;
                            worksheet.Cell("E" + rownum).Value = item.FoodCard;
                            worksheet.Cell("F" + rownum).Value = item.IBAN;
                            worksheet.Cell("G" + rownum).Value = item.BankName;
                            worksheet.Cell("H" + rownum).Value = item.FoodCardTotal;
                            worksheet.Cell("I" + rownum).Value = item.FoodCardPaymentAmount;
                            worksheet.Cell("J" + rownum).Value = item.FoodCardTotal;
                            worksheet.Cell("K" + rownum).Value = item.RecordDate;

                            rownum++;
                        }

                        worksheet.Cell("A" + rownum).Value = null;
                        worksheet.Cell("B" + rownum).Value = "Toplam";
                        worksheet.Cell("C" + rownum).Value = null;
                        worksheet.Cell("D" + rownum).Value = null;
                        worksheet.Cell("E" + rownum).Value = null;
                        worksheet.Cell("F" + rownum).Value = null;
                        worksheet.Cell("G" + rownum).Value = null;
                        worksheet.Cell("H" + rownum).Value = model.SalaryPeriodComputeSum.FoodCardTotal;
                        worksheet.Cell("I" + rownum).Value = model.SalaryPeriodComputeSum.FoodCardPaymentAmount;
                        worksheet.Cell("J" + rownum).Value = model.SalaryPeriodComputeSum.FoodCardTotal;
                        worksheet.Cell("K" + rownum).Value = null;

                        workbook.SaveAs(pathToExcelFile);
                    }

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return File(pathToExcelFile, "application/vnd.ms-excel", FileName);
        }

        [AllowAnonymous]
        public FileResult ExportDataPeriodBank(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            if (TempData["model"] != null)
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            string FileName = "Banka_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            string targetpath = Server.MapPath("~/Document/Salary/");
            string pathToExcelFile = targetpath + FileName;

            var salaryPeriod = Db.SalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod.ID == salaryPeriod.ID)
            {

                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Banka");

                        worksheet.Cell("A1").Value = "İsim";
                        worksheet.Cell("B1").Value = "TCKN(Opsiyonel)";
                        worksheet.Cell("C1").Value = "Banka Kodu";
                        worksheet.Cell("D1").Value = "Şube Kodu";
                        worksheet.Cell("E1").Value = "Hesap";
                        worksheet.Cell("F1").Value = "IBAN(Boşluksuz 26 Karakter)";
                        worksheet.Cell("G1").Value = "Tutar";
                        worksheet.Cell("H1").Value = "Borç İzahat";
                        worksheet.Cell("I1").Value = "Alacak izahat";

                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in model.SalaryPeriodComputes.Where(x=> x.SalaryPaymentTypeID == 1).OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.FullName;
                            worksheet.Cell("B" + rownum).Value = item.IdentityNumber;
                            worksheet.Cell("C" + rownum).Value = null;
                            worksheet.Cell("D" + rownum).Value = null;
                            worksheet.Cell("E" + rownum).Value = item.BankName;
                            worksheet.Cell("F" + rownum).Value = item.IBAN;
                            worksheet.Cell("G" + rownum).Value = item.TotalBalance;
                            worksheet.Cell("H" + rownum).Value = null;
                            worksheet.Cell("I" + rownum).Value = null;

                            rownum++;
                        }

                        workbook.SaveAs(pathToExcelFile);
                    }

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return File(pathToExcelFile, "application/vnd.ms-excel", FileName);
        }

        [AllowAnonymous]
        public FileResult GetDataPeriodTemplate(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return null;
            }


            if (TempData["model"] == null)
            {
                return null;
            }
            else
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (salaryPeriod != null && model.SalaryPeriod.ID == salaryPeriod.ID)
            {
                string targetpath = Server.MapPath("~/Document/Salary/");
                string FileName = $"SalaryPeriod{model.SalaryPeriod.ID}.xlsx";

                var isCreated = CreateExcelEarn(FileName, model.SalaryPeriodComputes, salaryPeriod.GroupType.Value);

                if (isCreated == true)
                {
                    string path = targetpath + FileName;
                    return File(path, "application/vnd.ms-excel", FileName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }


        }

        [AllowAnonymous]
        public FileResult GetDataPeriodPaymentTemplate(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return null;
            }


            if (TempData["model"] == null)
            {
                return null;
            }
            else
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (salaryPeriod != null && model.SalaryPeriod.ID == salaryPeriod.ID)
            {
                string targetpath = Server.MapPath("~/Document/Salary/");
                string FileName = $"SalaryPeriodPayment{model.SalaryPeriod.ID}.xlsx";

                var isCreated = CreateExcelPayment(FileName, model.SalaryPeriodComputes, salaryPeriod.GroupType.Value);

                if (isCreated == true)
                {
                    string path = targetpath + FileName;
                    return File(path, "application/vnd.ms-excel", FileName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }


        }

        public bool CreateExcelEarn(string FileName, List<SalaryPeriodCompute> SalaryRows, short SalaryPeriodGroupType)  // SalaryPeriodGroupType; 1 maaş, 2 setcard
        {
            bool isSuccess = false;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("SalaryPeriod");

                    if (SalaryPeriodGroupType == 1)
                    {
                        worksheet.Cell("A1").Value = "SalaryPeriodID";
                        worksheet.Cell("B1").Value = "EmployeeID";
                        worksheet.Cell("C1").Value = "EmployeeName";
                        worksheet.Cell("D1").Value = "SalaryTotal";
                        worksheet.Cell("E1").Value = "PermitTotal";
                        worksheet.Cell("F1").Value = "ExtraShiftTotal";
                        worksheet.Cell("G1").Value = "PremiumTotal";
                        worksheet.Cell("H1").Value = "FormalTotal";
                        worksheet.Cell("I1").Value = "OtherTotal";
                        worksheet.Cell("J1").Value = "PrePaymentAmount";
                        worksheet.Cell("K1").Value = "SalaryCutAmount";
                        worksheet.Cell("L1").Value = "PermitPaymentAmount";
                        worksheet.Cell("M1").Value = "ExtraShiftPaymentAmount";
                        worksheet.Cell("N1").Value = "PremiumPaymentAmount";
                        worksheet.Cell("O1").Value = "FormalPaymentAmount";
                        worksheet.Cell("P1").Value = "OtherPaymentAmount";
                        worksheet.Cell("Q1").Value = "Currency";

                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in SalaryRows.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                            worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("C" + rownum).Value = item.FullName;
                            worksheet.Cell("D" + rownum).Value = item.SalaryTotal;
                            worksheet.Cell("E" + rownum).Value = item.PermitTotal;
                            worksheet.Cell("F" + rownum).Value = item.ExtraShiftTotal;
                            worksheet.Cell("G" + rownum).Value = item.PremiumTotal;
                            worksheet.Cell("H" + rownum).Value = item.FormalTotal;
                            worksheet.Cell("I" + rownum).Value = item.OtherTotal;
                            worksheet.Cell("J" + rownum).Value = item.PrePaymentAmount;
                            worksheet.Cell("K" + rownum).Value = item.SalaryCutAmount;
                            worksheet.Cell("L" + rownum).Value = item.PermitPaymentAmount;
                            worksheet.Cell("M" + rownum).Value = item.ExtraShiftPaymentAmount;
                            worksheet.Cell("N" + rownum).Value = item.PremiumPaymentAmount;
                            worksheet.Cell("O" + rownum).Value = item.FormalPaymentAmount;
                            worksheet.Cell("P" + rownum).Value = item.OtherPaymentAmount;
                            worksheet.Cell("Q" + rownum).Value = item.Currency;

                            rownum++;
                        }
                    }

                    if (SalaryPeriodGroupType == 2)
                    {
                        worksheet.Cell("A1").Value = "SalaryPeriodID";
                        worksheet.Cell("B1").Value = "EmployeeID";
                        worksheet.Cell("C1").Value = "EmployeeName";
                        worksheet.Cell("D1").Value = "FoodCardTotal";
                        worksheet.Cell("E1").Value = "FoodCardPaymentAmount";
                        worksheet.Cell("F1").Value = "Currency";

                        //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                        int rownum = 2;

                        foreach (var item in SalaryRows.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                            worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("C" + rownum).Value = item.FullName;
                            worksheet.Cell("D" + rownum).Value = item.FoodCardTotal;
                            worksheet.Cell("E" + rownum).Value = item.FoodCardPaymentAmount;
                            worksheet.Cell("F" + rownum).Value = item.Currency; ;

                            rownum++;
                        }
                    }



                    string targetpath = Server.MapPath("~/Document/Salary/");
                    string pathToExcelFile = targetpath + FileName;
                    workbook.SaveAs(pathToExcelFile);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        public bool CreateExcelPayment(string FileName, List<SalaryPeriodCompute> SalaryRows, short SalaryPeriodGroupType)  // SalaryPeriodGroupType; 1 maaş, 2 setcard
        {
            bool isSuccess = false;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("SalaryPeriodPayment");

                    if (SalaryPeriodGroupType == 1)
                    {


                        worksheet.Cell("A1").Value = "SalaryPeriodID";
                        worksheet.Cell("B1").Value = "EmployeeID";
                        worksheet.Cell("C1").Value = "EmployeeName";
                        worksheet.Cell("D1").Value = "BankPaymentAmount";
                        worksheet.Cell("E1").Value = "ManuelPaymentAmount";
                        worksheet.Cell("F1").Value = "TransferBalance";
                        worksheet.Cell("G1").Value = "Currency";

                        int rownum = 2;

                        foreach (var item in SalaryRows.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                            worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("C" + rownum).Value = item.FullName;
                            worksheet.Cell("D" + rownum).Value = item.BankPaymentAmount;
                            worksheet.Cell("E" + rownum).Value = item.ManuelPaymentAmount;
                            worksheet.Cell("F" + rownum).Value = item.TransferBalance;
                            worksheet.Cell("G" + rownum).Value = item.Currency;

                            rownum++;
                        }
                    }

                    if (SalaryPeriodGroupType == 2)
                    {
                        worksheet.Cell("A1").Value = "SalaryPeriodID";
                        worksheet.Cell("B1").Value = "EmployeeID";
                        worksheet.Cell("C1").Value = "EmployeeName";
                        worksheet.Cell("D1").Value = "FoodCardTotal";
                        worksheet.Cell("E1").Value = "FoodCardPaymentAmount";
                        worksheet.Cell("F1").Value = "Currency";

                        int rownum = 2;

                        foreach (var item in SalaryRows.OrderBy(x => x.FullName))
                        {

                            worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                            worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                            worksheet.Cell("C" + rownum).Value = item.FullName;
                            worksheet.Cell("D" + rownum).Value = item.FoodCardTotal;
                            worksheet.Cell("E" + rownum).Value = item.FoodCardPaymentAmount;
                            worksheet.Cell("F" + rownum).Value = item.Currency; ;

                            rownum++;
                        }
                    }


                    string targetpath = Server.MapPath("~/Document/Salary/");
                    string pathToExcelFile = targetpath + FileName;
                    workbook.SaveAs(pathToExcelFile);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        public bool CreateExcelCost(string FileName, List<SalaryPeriodCompute> SalaryRows)
        {
            bool isSuccess = false;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("SalaryPeriodCost");




                    worksheet.Cell("A1").Value = "SalaryPeriodID";
                    worksheet.Cell("B1").Value = "EmployeeID";
                    worksheet.Cell("C1").Value = "Identity";
                    worksheet.Cell("D1").Value = "SGKBranch";
                    worksheet.Cell("E1").Value = "LocationName";
                    worksheet.Cell("F1").Value = "EmployeeName";
                    worksheet.Cell("G1").Value = "NetCost";
                    worksheet.Cell("H1").Value = "SSK";
                    worksheet.Cell("I1").Value = "GV";
                    worksheet.Cell("J1").Value = "DV";


                    int rownum = 2;

                    foreach (var item in SalaryRows.Distinct().OrderBy(x => x.FullName))
                    {

                        worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                        worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                        worksheet.Cell("C" + rownum).Value = item.IdentityNumber;
                        worksheet.Cell("D" + rownum).Value = item.SGKBranch;
                        worksheet.Cell("E" + rownum).Value = item.LocationName;
                        worksheet.Cell("F" + rownum).Value = item.FullName;
                        worksheet.Cell("G" + rownum).Value = item.NetCost;
                        worksheet.Cell("H" + rownum).Value = item.SSK;
                        worksheet.Cell("I" + rownum).Value = item.GV;
                        worksheet.Cell("J" + rownum).Value = item.DV;

                        rownum++;
                    }

                    string targetpath = Server.MapPath("~/Document/Salary/");
                    string pathToExcelFile = targetpath + FileName;
                    workbook.SaveAs(pathToExcelFile);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }


        //GetDataPeriodCostTemplate
        [AllowAnonymous]
        public FileResult GetDataPeriodCostTemplate(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return null;
            }


            if (TempData["model"] == null)
            {
                return null;
            }
            else
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (salaryPeriod != null && model.SalaryPeriod.ID == salaryPeriod.ID)
            {
                string targetpath = Server.MapPath("~/Document/Salary/");
                string FileName = $"SalaryPeriodCost{model.SalaryPeriod.ID}.xlsx";

                var isCreated = CreateExcelCost(FileName, model.SalaryPeriodComputes);

                if (isCreated == true)
                {
                    string path = targetpath + FileName;
                    return File(path, "application/vnd.ms-excel", FileName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }


        }

        [AllowAnonymous]
        public FileResult GetDataFoodcardEarnTemplate(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return null;
            }

            if (TempData["model"] == null)
            {
                return null;
            }
            else
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (salaryPeriod != null && model.SalaryPeriod.ID == salaryPeriod.ID)
            {
                string targetpath = Server.MapPath("~/Document/Salary/");
                string FileName = $"FoodcardEarn{model.SalaryPeriod.ID}.xlsx";

                var isCreated = CreateExcelFoodEarn(FileName, model.SalaryPeriodComputes);

                if (isCreated == true)
                {
                    string path = targetpath + FileName;
                    return File(path, "application/vnd.ms-excel", FileName);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }


        }

        [AllowAnonymous]
        public FileResult GetDataFoodcardPaymentTemplate(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return null;
            }


            if (TempData["model"] == null)
            {
                return null;
            }
            else
            {
                model = TempData["model"] as SalaryControlModel;
            }

            TempData["model"] = model;

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (salaryPeriod != null && model.SalaryPeriod.ID == salaryPeriod.ID)
            {
                string targetpath = Server.MapPath("~/Document/Salary/");
                string FileName = $"FoodcardPayment{model.SalaryPeriod.ID}.xlsx";

                var isCreated = CreateExcelFoodPayment(FileName, model.SalaryPeriodComputes);

                if (isCreated == true)
                {
                    string path = targetpath + FileName;
                    return File(path, "application/vnd.ms-excel", FileName);
                }
                else
                {
                    return null;
                }

            }
            else
            {
                return null;
            }


        }

        public bool CreateExcelFoodEarn(string FileName, List<SalaryPeriodCompute> SalaryRows)
        {
            bool isSuccess = false;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("FoodcardEarn");

                    worksheet.Cell("A1").Value = "SalaryPeriodID";
                    worksheet.Cell("B1").Value = "EmployeeID";
                    worksheet.Cell("C1").Value = "EmployeeName";
                    worksheet.Cell("D1").Value = "FoodCardTotal";
                    worksheet.Cell("E1").Value = "Currency";

                    int rownum = 2;

                    foreach (var item in SalaryRows.OrderBy(x => x.FullName))
                    {

                        worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                        worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                        worksheet.Cell("C" + rownum).Value = item.FullName;
                        worksheet.Cell("D" + rownum).Value = item.FoodCardTotal;
                        worksheet.Cell("E" + rownum).Value = item.Currency;

                        rownum++;
                    }

                    string targetpath = Server.MapPath("~/Document/Salary/");
                    string pathToExcelFile = targetpath + FileName;
                    workbook.SaveAs(pathToExcelFile);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        public bool CreateExcelFoodPayment(string FileName, List<SalaryPeriodCompute> SalaryRows)  // SalaryPeriodGroupType; 1 maaş, 2 setcard
        {
            bool isSuccess = false;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("FoodcardPayment");

                    worksheet.Cell("A1").Value = "SalaryPeriodID";
                    worksheet.Cell("B1").Value = "EmployeeID";
                    worksheet.Cell("C1").Value = "EmployeeName";
                    worksheet.Cell("D1").Value = "FoodCardPaymentAmount";
                    worksheet.Cell("E1").Value = "Currency";

                    //worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";

                    int rownum = 2;

                    foreach (var item in SalaryRows.OrderBy(x => x.FullName))
                    {

                        worksheet.Cell("A" + rownum).Value = item.SalaryPeriodID;
                        worksheet.Cell("B" + rownum).Value = item.EmployeeID;
                        worksheet.Cell("C" + rownum).Value = item.FullName;
                        worksheet.Cell("D" + rownum).Value = item.FoodCardPaymentAmount;
                        worksheet.Cell("E" + rownum).Value = item.Currency;

                        rownum++;
                    }

                    string targetpath = Server.MapPath("~/Document/Salary/");
                    string pathToExcelFile = targetpath + FileName;
                    workbook.SaveAs(pathToExcelFile);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
            }

            return isSuccess;
        }


        [AllowAnonymous]
        public ActionResult ImportSalaryEarn(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }


            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }



            return View(model);


        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportSalaryEarnFile(FormSalaryPeriodEarnImport form)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty,
                InfoKeyList = new List<InfoKey>()
            };

            if (form == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == form.SalaryPeriodID);
            var datalistforlog = new List<ExcelSalaryPeriodEarn>();

            if (salaryPeriod != null)
            {
                var salaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == salaryPeriod.ID).ToList();
                List<SalaryPeriodCompute> computeList = new List<SalaryPeriodCompute>();


                if (form.SalaryFile != null && form.SalaryFile.ContentLength > 0)
                {

                    if (form.SalaryFile.ContentType == "application/vnd.ms-excel" || form.SalaryFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.SalaryFile.FileName);
                        string targetpath = Server.MapPath("~/Document/Salary/");
                        string pathToExcelFile = targetpath + filename;


                        form.SalaryFile.SaveAs(Path.Combine(targetpath, filename));



                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }


                        //var adapter = new OleDbDataAdapter("SELECT * FROM [SalaryPeriod$]", connectionString);
                        //var ds = new DataSet();
                        //adapter.Fill(ds, "ExcelTable");
                        //DataTable dtable = ds.Tables["ExcelTable"];


                        string sheetName = "SalaryPeriod";
                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var salaryList = from a in excelFile.Worksheet<ExcelSalaryPeriodEarn>(sheetName) select a;
                        datalistforlog = salaryList.ToList();

                        foreach (var item in datalistforlog)
                        {
                            var compute = salaryPeriodComputes.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);

                            try
                            {
                                if (compute != null)
                                {
                                    compute.SalaryTotal = item.SalaryTotal != null ? item.SalaryTotal : compute.SalaryTotal;
                                    compute.PermitTotal = item.PermitTotal != null ? item.PermitTotal : compute.PermitTotal;
                                    compute.ExtraShiftTotal = item.ExtraShiftTotal != null ? item.ExtraShiftTotal : compute.ExtraShiftTotal;
                                    compute.PremiumTotal = item.PremiumTotal != null ? item.PremiumTotal : compute.PremiumTotal;
                                    compute.FormalTotal = item.FormalTotal != null ? item.FormalTotal : compute.FormalTotal;
                                    compute.OtherTotal = item.OtherTotal != null ? item.OtherTotal : compute.OtherTotal;


                                    compute.PrePaymentAmount = item.PrePaymentAmount != null ? item.PrePaymentAmount : compute.PrePaymentAmount;
                                    compute.SalaryCutAmount = item.SalaryCutAmount != null ? item.SalaryCutAmount : compute.SalaryCutAmount;
                                    compute.PermitPaymentAmount = item.PermitPaymentAmount != null ? item.PermitPaymentAmount : compute.PermitPaymentAmount;
                                    compute.ExtraShiftPaymentAmount = item.ExtraShiftPaymentAmount != null ? item.ExtraShiftPaymentAmount : compute.ExtraShiftPaymentAmount;
                                    compute.PremiumPaymentAmount = item.PremiumPaymentAmount != null ? item.PremiumPaymentAmount : compute.PremiumPaymentAmount;
                                    compute.FormalPaymentAmount = item.FormalPaymentAmount != null ? item.FormalPaymentAmount : compute.FormalPaymentAmount;
                                    compute.OtherPaymentAmount = item.OtherPaymentAmount != null ? item.OtherPaymentAmount : compute.OtherPaymentAmount;

                                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Güncellendi" });
                                }
                                else
                                {
                                    var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);
                                    string bankName = "";

                                    if (employee != null)
                                    {
                                        bankName = Db.Bank.FirstOrDefault(x => x.ID == employee.BankID)?.ShortName ?? "";

                                        SalaryPeriodCompute newcompute = new SalaryPeriodCompute();

                                        newcompute.SalaryPeriodID = salaryPeriod.ID;
                                        newcompute.EmployeeID = employee.EmployeeID;
                                        newcompute.FullName = employee.FullName;
                                        newcompute.IdentityNumber = employee.IdentityNumber;
                                        newcompute.PhoneNumber = employee.CountryPhoneCode + employee.SmsNumber;
                                        newcompute.FoodCard = employee.FoodCardNumber;
                                        newcompute.IBAN = employee.IBAN;
                                        newcompute.BankName = bankName;
                                        newcompute.SalaryPaymentTypeID = employee.SalaryPaymentTypeID;

                                        newcompute.SalaryTotal = item.SalaryTotal != null ? item.SalaryTotal : 0;
                                        newcompute.PermitTotal = item.PermitTotal != null ? item.PermitTotal : 0;
                                        newcompute.ExtraShiftTotal = item.ExtraShiftTotal != null ? item.ExtraShiftTotal : 0;
                                        newcompute.PremiumTotal = item.PremiumTotal != null ? item.PremiumTotal : 0;
                                        newcompute.FormalTotal = item.FormalTotal != null ? item.FormalTotal : 0;
                                        newcompute.OtherTotal = item.OtherTotal != null ? item.OtherTotal : 0;

                                        newcompute.PrePaymentAmount = item.PrePaymentAmount != null ? item.PrePaymentAmount : 0;
                                        newcompute.SalaryCutAmount = item.SalaryCutAmount != null ? item.SalaryCutAmount : 0;
                                        newcompute.PermitPaymentAmount = item.PermitPaymentAmount != null ? item.PermitPaymentAmount : 0;
                                        newcompute.ExtraShiftPaymentAmount = item.ExtraShiftPaymentAmount != null ? item.ExtraShiftPaymentAmount : 0;
                                        newcompute.PremiumPaymentAmount = item.PremiumPaymentAmount != null ? item.PremiumPaymentAmount : 0;
                                        newcompute.FormalPaymentAmount = item.FormalPaymentAmount != null ? item.FormalPaymentAmount : 0;
                                        newcompute.OtherPaymentAmount = item.OtherPaymentAmount != null ? item.OtherPaymentAmount : 0;

                                        newcompute.FoodCardTotal = 0;
                                        newcompute.FoodCardPaymentAmount = 0;

                                        newcompute.BankPaymentAmount = 0;
                                        newcompute.ManuelPaymentAmount = 0;
                                        newcompute.TransferBalance = 0;

                                        newcompute.NetCost = 0;
                                        newcompute.SSK = 0;
                                        newcompute.GV = 0;
                                        newcompute.DV = 0;

                                        newcompute.RecordDate = DateTime.UtcNow.AddHours(3);
                                        newcompute.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                                        newcompute.RecordIP = OfficeHelper.GetIPAddress();
                                        newcompute.UID = Guid.NewGuid();
                                        newcompute.Currency = item.Currency;

                                        computeList.Add(newcompute);

                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Eklendi" });
                                    }
                                }
                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                {
                                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                                    {
                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = validationError.PropertyName, Message = validationError.ErrorMessage });
                                    }
                                }
                            }
                        }

                        Db.SaveChanges();

                        Db.SalaryPeriodCompute.AddRange(computeList);
                        Db.SaveChanges();

                        Db.SalaryPeriodComputeSetTotal(form.SalaryPeriodID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

                        //deleting excel file from folder
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }
                    }
                    else
                    {
                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Format Hatası", Message = "Sadece Excel Dosyası Geçerlidir." });
                    }
                }
                else
                {
                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Dosya Hatası", Message = "Excel Dosyası Seçin." });
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";

            TempData["result"] = model.Result;
            OfficeHelper.AddApplicationLog("Office", "Salary", "Import", form.SalaryPeriodID.ToString(), "Salary", "ImportSalaryEarnFile", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, datalistforlog);

            return RedirectToAction("ImportSalaryEarn", new { id = salaryPeriod.UID });

        }



        [AllowAnonymous]
        public ActionResult ImportSalaryPayment(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }


            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }



            return View(model);


        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportSalaryPaymentFile(FormSalaryPeriodEarnImport form)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty,
                InfoKeyList = new List<InfoKey>()
            };

            if (form == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == form.SalaryPeriodID);
            var datalistforlog = new List<ExcelSalaryPeriodPayment>();

            if (salaryPeriod != null)
            {
                var salaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == salaryPeriod.ID).ToList();
                List<SalaryPeriodCompute> computeList = new List<SalaryPeriodCompute>();


                if (form.SalaryFile != null && form.SalaryFile.ContentLength > 0)
                {

                    if (form.SalaryFile.ContentType == "application/vnd.ms-excel" || form.SalaryFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.SalaryFile.FileName);
                        string targetpath = Server.MapPath("~/Document/Salary/");
                        string pathToExcelFile = targetpath + filename;


                        form.SalaryFile.SaveAs(Path.Combine(targetpath, filename));



                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }


                        //var adapter = new OleDbDataAdapter("SELECT * FROM [SalaryPeriodPayment$]", connectionString);
                        //var ds = new DataSet();
                        //adapter.Fill(ds, "ExcelTable");
                        //DataTable dtable = ds.Tables["ExcelTable"];


                        string sheetName = "SalaryPeriodPayment";
                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var salaryList = from a in excelFile.Worksheet<ExcelSalaryPeriodPayment>(sheetName) select a;
                        datalistforlog = salaryList.ToList();

                        foreach (var item in datalistforlog)
                        {
                            var compute = salaryPeriodComputes.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);

                            try
                            {
                                if (compute != null)
                                {
                                    compute.BankPaymentAmount = item.BankPaymentAmount != null ? item.BankPaymentAmount : compute.BankPaymentAmount;
                                    compute.ManuelPaymentAmount = item.ManuelPaymentAmount != null ? item.ManuelPaymentAmount : compute.ManuelPaymentAmount;
                                    compute.TransferBalance = item.TransferBalance != null ? item.TransferBalance : compute.TransferBalance;
                                    compute.Currency = item.Currency;

                                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Güncellendi" });
                                }
                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                {
                                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                                    {
                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = validationError.PropertyName, Message = validationError.ErrorMessage });
                                    }
                                }
                            }
                        }

                        Db.SaveChanges();

                        Db.SalaryPeriodCompute.AddRange(computeList);
                        Db.SaveChanges();

                        Db.SalaryPeriodComputeSetTotal(form.SalaryPeriodID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

                        //deleting excel file from folder
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }
                    }
                    else
                    {
                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Format Hatası", Message = "Sadece Excel Dosyası Geçerlidir." });
                    }
                }
                else
                {
                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Dosya Hatası", Message = "Excel Dosyası Seçin." });
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";

            TempData["result"] = model.Result;
            OfficeHelper.AddApplicationLog("Office", "Salary", "Import", form.SalaryPeriodID.ToString(), "Salary", "ImportSalaryPaymentFile", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, datalistforlog);

            return RedirectToAction("ImportSalaryPayment", new { id = salaryPeriod.UID });

        }

        //

        [AllowAnonymous]
        public ActionResult ImportSalaryCost(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }


            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }



            return View(model);


        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportSalaryCostFile(FormSalaryPeriodEarnImport form)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty,
                InfoKeyList = new List<InfoKey>()
            };

            if (form == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == form.SalaryPeriodID);
            var datalistforlog = new List<ExcelSalaryPeriodCost>();

            if (salaryPeriod != null)
            {
                var salaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == salaryPeriod.ID).ToList();
                List<SalaryPeriodCompute> computeList = new List<SalaryPeriodCompute>();


                if (form.SalaryFile != null && form.SalaryFile.ContentLength > 0)
                {

                    if (form.SalaryFile.ContentType == "application/vnd.ms-excel" || form.SalaryFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.SalaryFile.FileName);
                        string targetpath = Server.MapPath("~/Document/Salary/");
                        string pathToExcelFile = targetpath + filename;


                        form.SalaryFile.SaveAs(Path.Combine(targetpath, filename));



                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }


                        //var adapter = new OleDbDataAdapter("SELECT * FROM [SalaryPeriodPayment$]", connectionString);
                        //var ds = new DataSet();
                        //adapter.Fill(ds, "ExcelTable");
                        //DataTable dtable = ds.Tables["ExcelTable"];


                        string sheetName = "SalaryPeriodCost";
                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var salaryList = from a in excelFile.Worksheet<ExcelSalaryPeriodCost>(sheetName) select a;
                        datalistforlog = salaryList.ToList();

                        foreach (var item in datalistforlog)
                        {
                            var compute = salaryPeriodComputes.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);

                            try
                            {
                                if (compute != null)
                                {
                                    compute.NetCost = item.NetCost != null ? item.NetCost : compute.NetCost;
                                    compute.SSK = item.SSK != null ? item.SSK : compute.SSK;
                                    compute.GV = item.GV != null ? item.GV : compute.GV;
                                    compute.DV = item.DV != null ? item.DV : compute.GV;
                                    compute.CostDate = DateTime.UtcNow.AddHours(3);

                                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Güncellendi" });
                                }
                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                {
                                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                                    {
                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = validationError.PropertyName, Message = validationError.ErrorMessage });
                                    }
                                }
                            }
                        }

                        Db.SaveChanges();

                        Db.SalaryPeriodCompute.AddRange(computeList);
                        Db.SaveChanges();

                        Db.SalaryPeriodComputeSetTotal(form.SalaryPeriodID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());

                        //deleting excel file from folder
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }
                    }
                    else
                    {
                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Format Hatası", Message = "Sadece Excel Dosyası Geçerlidir." });
                    }
                }
                else
                {
                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Dosya Hatası", Message = "Excel Dosyası Seçin." });
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Maaş Periyodu bilgisi bulunamadı";

            TempData["result"] = model.Result;
            OfficeHelper.AddApplicationLog("Office", "Salary", "Import", form.SalaryPeriodID.ToString(), "Salary", "ImportSalaryCostFile", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, datalistforlog);

            return RedirectToAction("ImportSalaryCost", new { id = salaryPeriod.UID });

        }






        [AllowAnonymous]
        public ActionResult ImportFoodcardEarn(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }


            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }



            return View(model);


        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportFoodcardEarnFile(FormSalaryPeriodEarnImport form)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty,
                InfoKeyList = new List<InfoKey>()
            };

            if (form == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == form.SalaryPeriodID);
            var datalistforlog = new List<ExcelSalaryPeriodFoodEarn>();

            if (salaryPeriod != null)
            {
                var salaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == salaryPeriod.ID).ToList();
                List<SalaryPeriodCompute> computeList = new List<SalaryPeriodCompute>();


                if (form.SalaryFile != null && form.SalaryFile.ContentLength > 0)
                {

                    if (form.SalaryFile.ContentType == "application/vnd.ms-excel" || form.SalaryFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.SalaryFile.FileName);
                        string targetpath = Server.MapPath("~/Document/Salary/");
                        string pathToExcelFile = targetpath + filename;

                        form.SalaryFile.SaveAs(Path.Combine(targetpath, filename));

                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }

                        //var adapter = new OleDbDataAdapter("SELECT * FROM [SalaryPeriod$]", connectionString);
                        //var ds = new DataSet();
                        //adapter.Fill(ds, "ExcelTable");
                        //DataTable dtable = ds.Tables["ExcelTable"];

                        string sheetName = "FoodcardEarn";
                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var salaryList = from a in excelFile.Worksheet<ExcelSalaryPeriodFoodEarn>(sheetName) select a;
                        datalistforlog = salaryList.ToList();

                        foreach (var item in datalistforlog)
                        {
                            var compute = salaryPeriodComputes.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);

                            try
                            {
                                if (compute != null)
                                {

                                    compute.FoodCardTotal = item.FoodCardTotal != null ? item.FoodCardTotal : compute.FoodCardTotal;

                                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Güncellendi" });
                                }
                                else
                                {
                                    var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);
                                    string bankName = "";

                                    if (employee != null)
                                    {
                                        bankName = Db.Bank.FirstOrDefault(x => x.ID == employee.BankID)?.ShortName ?? "";

                                        SalaryPeriodCompute newcompute = new SalaryPeriodCompute();

                                        newcompute.SalaryPeriodID = salaryPeriod.ID;
                                        newcompute.EmployeeID = employee.EmployeeID;
                                        newcompute.FullName = employee.FullName;
                                        newcompute.IdentityNumber = employee.IdentityNumber;
                                        newcompute.PhoneNumber = employee.CountryPhoneCode + employee.SmsNumber;
                                        newcompute.FoodCard = employee.FoodCardNumber;
                                        newcompute.IBAN = employee.IBAN;
                                        newcompute.BankName = bankName;
                                        newcompute.SalaryPaymentTypeID = employee.SalaryPaymentTypeID;

                                        newcompute.SalaryTotal = 0;
                                        newcompute.PermitTotal = 0;
                                        newcompute.ExtraShiftTotal = 0;
                                        newcompute.PremiumTotal = 0;
                                        newcompute.FormalTotal = 0;
                                        newcompute.OtherTotal = 0;

                                        newcompute.PrePaymentAmount = 0;
                                        newcompute.SalaryCutAmount = 0;
                                        newcompute.PermitPaymentAmount = 0;
                                        newcompute.ExtraShiftPaymentAmount = 0;
                                        newcompute.PremiumPaymentAmount = 0;
                                        newcompute.FormalPaymentAmount = 0;
                                        newcompute.OtherPaymentAmount = 0;

                                        newcompute.BankPaymentAmount = 0;
                                        newcompute.ManuelPaymentAmount = 0;
                                        newcompute.TransferBalance = 0;
                                        newcompute.FoodCardTotal = item.FoodCardTotal != null ? item.FoodCardTotal : 0;
                                        newcompute.FoodCardPaymentAmount = 0;

                                        newcompute.NetCost = 0;
                                        newcompute.SSK = 0;
                                        newcompute.GV = 0;
                                        newcompute.DV = 0;

                                        newcompute.RecordDate = DateTime.UtcNow.AddHours(3);
                                        newcompute.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                                        newcompute.RecordIP = OfficeHelper.GetIPAddress();
                                        newcompute.UID = Guid.NewGuid();
                                        newcompute.Currency = item.Currency;

                                        computeList.Add(newcompute);

                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Eklendi" });
                                    }
                                }
                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                {
                                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                                    {
                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = validationError.PropertyName, Message = validationError.ErrorMessage });
                                    }
                                }
                            }
                        }

                        Db.SaveChanges();

                        Db.SalaryPeriodCompute.AddRange(computeList);
                        Db.SaveChanges();

                        Db.SalaryPeriodFoodComputeSetTotal(form.SalaryPeriodID);

                        //deleting excel file from folder
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }
                    }
                    else
                    {
                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Format Hatası", Message = "Sadece Excel Dosyası Geçerlidir." });
                    }
                }
                else
                {
                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Dosya Hatası", Message = "Excel Dosyası Seçin." });
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"Yemek Kartı Periyodu bilgisi bulunamadı";
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Yemek Kartı Periyodu bilgisi bulunamadı";

            TempData["result"] = model.Result;
            OfficeHelper.AddApplicationLog("Office", "Salary", "Import", form.SalaryPeriodID.ToString(), "Salary", "ImportFoodcardEarnFile", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, datalistforlog);

            return RedirectToAction("ImportFoodcardEarn", new { id = salaryPeriod.UID });

        }


        [AllowAnonymous]
        public ActionResult ImportFoodcardPayment(Guid? id)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result();

            var allowedempids = new int[] { 1, 19, 3921, 129, 4679, 4038, 396, 4147 }.ToList();

            if (!allowedempids.Contains(model.Authentication.ActionEmployee.EmployeeID))
            {
                return RedirectToAction("Index");
            }

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result;
            }


            if (id == null)
            {
                return RedirectToAction("SalaryResult");
            }

            model.SalaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.UID == id);

            if (model.SalaryPeriod == null)
            {
                return RedirectToAction("SalaryResult");
            }



            return View(model);


        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ImportFoodcardPaymentFile(FormSalaryPeriodEarnImport form)
        {
            SalaryControlModel model = new SalaryControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty,
                InfoKeyList = new List<InfoKey>()
            };

            if (form == null)
            {
                return RedirectToAction("SalaryResult");
            }

            var salaryPeriod = Db.VSalaryPeriod.FirstOrDefault(x => x.ID == form.SalaryPeriodID);
            var datalistforlog = new List<ExcelSalaryPeriodFoodPayment>();

            if (salaryPeriod != null)
            {
                var salaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == salaryPeriod.ID).ToList();
                List<SalaryPeriodCompute> computeList = new List<SalaryPeriodCompute>();


                if (form.SalaryFile != null && form.SalaryFile.ContentLength > 0)
                {

                    if (form.SalaryFile.ContentType == "application/vnd.ms-excel" || form.SalaryFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.SalaryFile.FileName);
                        string targetpath = Server.MapPath("~/Document/Salary/");
                        string pathToExcelFile = targetpath + filename;

                        form.SalaryFile.SaveAs(Path.Combine(targetpath, filename));

                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }

                        //var adapter = new OleDbDataAdapter("SELECT * FROM [SalaryPeriod$]", connectionString);
                        //var ds = new DataSet();
                        //adapter.Fill(ds, "ExcelTable");
                        //DataTable dtable = ds.Tables["ExcelTable"];

                        string sheetName = "FoodcardPayment";
                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var salaryList = from a in excelFile.Worksheet<ExcelSalaryPeriodFoodPayment>(sheetName) select a;
                        datalistforlog = salaryList.ToList();

                        foreach (var item in datalistforlog)
                        {
                            var compute = salaryPeriodComputes.FirstOrDefault(x => x.EmployeeID == item.EmployeeID);

                            try
                            {
                                if (compute != null)
                                {

                                    compute.FoodCardPaymentAmount = item.FoodCardPaymentAmount != null ? item.FoodCardPaymentAmount : compute.FoodCardPaymentAmount;

                                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = true, Name = $"{item.EmployeeName}", Message = $"Güncellendi" });
                                }

                            }
                            catch (DbEntityValidationException ex)
                            {
                                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                                {
                                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                                    {
                                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = validationError.PropertyName, Message = validationError.ErrorMessage });
                                    }
                                }
                            }
                        }

                        Db.SaveChanges();

                        Db.SalaryPeriodCompute.AddRange(computeList);
                        Db.SaveChanges();

                        Db.SalaryPeriodFoodComputeSetTotal(form.SalaryPeriodID);

                        //deleting excel file from folder
                        if ((System.IO.File.Exists(pathToExcelFile)))
                        {
                            System.IO.File.Delete(pathToExcelFile);
                        }
                    }
                    else
                    {
                        model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Format Hatası", Message = "Sadece Excel Dosyası Geçerlidir." });
                    }
                }
                else
                {
                    model.Result.InfoKeyList.Add(new InfoKey() { IsSuccess = false, Name = "Dosya Hatası", Message = "Excel Dosyası Seçin." });
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = $"Yemek Kartı Periyodu bilgisi bulunamadı";
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Yemek Kartı Periyodu bilgisi bulunamadı";

            TempData["result"] = model.Result;
            OfficeHelper.AddApplicationLog("Office", "Salary", "Import", form.SalaryPeriodID.ToString(), "Salary", "ImportFoodcardPaymentFile", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, datalistforlog);

            return RedirectToAction("ImportFoodcardPayment", new { id = salaryPeriod.UID });

        }









    }
}