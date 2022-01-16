using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public ActionResult SalaryResult(int? EmployeeID, int? LocationID, int? SalaryPeriodID, DateTime? DateBegin, DateTime? DateEnd )
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
                filterModel.DateBegin = DateBegin != null ? DateBegin : DateTime.Now.AddDays(-15).Date;
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
                MobilePhone = x.MobilePhone
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
        public void ExportData()
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
                "attachment; filename=Maas_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls");
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


            View("~/Views/Salary/ReportView.cshtml", model).ExecuteResult(this.ControllerContext);
            Response.Flush();
            Response.End();
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
                return RedirectToAction("SalaryResult");
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

            model.Filters.DateBegin = model.SalaryPeriod.DateBegin;
            model.Filters.DateEnd = model.SalaryPeriod.DateEnd;

            model.SalaryPeriodComputes = Db.SalaryPeriodCompute.Where(x => x.SalaryPeriodID == model.SalaryPeriod.ID).ToList();
            



            return View(model);
        }


    }
}