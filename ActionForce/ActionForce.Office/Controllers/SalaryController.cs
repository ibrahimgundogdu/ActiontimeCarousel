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
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 1 && x.IsActive == true).ToList();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);
            model.UnitPrice = Db.EmployeeSalary.ToList();
            model.SalaryEarn = Db.VDocumentSalaryEarn.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
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


                var currency = cashSalary.Currency;
                var docDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashSalary.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashSalary.DocumentDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashSalary.LocationID, cashSalary.Currency);
                // tahsilat eklenir.
                double? quantity = cashSalary.QuantityHour;
                double? price = cashSalary.UnitPrice;
                if (quantity > 0 && price > 0)
                {
                    SalaryEarn earn = new SalaryEarn();

                    earn.ActionTypeID = actType.ID;
                    earn.ActionTypeName = actType.Name;
                    earn.Currency = cashSalary.Currency;
                    earn.Description = cashSalary.Description;
                    earn.DocumentDate = docDate;
                    earn.EmployeeID = fromID;
                    earn.EnvironmentID = 2;
                    earn.LocationID = location.LocationID;
                    earn.QuantityHour = quantity;
                    earn.TotalAmount = (double)((double)cashSalary.QuantityHour * (double?)cashSalary.UnitPrice);
                    earn.UID = Guid.NewGuid();
                    earn.UnitPrice = (double?)cashSalary.UnitPrice;
                    earn.TimeZone = location.Timezone;
                    earn.OurCompanyID = location.OurCompanyID;
                    earn.CategoryID = cashSalary.CategoryID ?? (int?)null;


                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddSalaryEarn(earn, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
                

            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;

            return RedirectToAction("Index", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryEarn(NewSalaryEarn cashEarn)
        {
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            SalaryControlModel model = new SalaryControlModel();


            if (cashEarn != null)
            {
                var fromPrefix = cashEarn.EmployeeID.Substring(0, 1);
                var fromID = Convert.ToInt32(cashEarn.EmployeeID.Substring(1, cashEarn.EmployeeID.Length - 1));
                var amount = Convert.ToDouble(cashEarn.TotalAmount.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var unit = Convert.ToDouble(cashEarn.UnitPrice.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var quantity = Convert.ToDouble(cashEarn.QuantityHour.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var currency = cashEarn.Currency;
                var docDate = DateTime.Now.Date;
                if (DateTime.TryParse(cashEarn.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashEarn.DocumentDate).Date;
                }

                var price = Convert.ToDouble(cashEarn.UnitPrice.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                if (quantity > 0 && price > 0)
                {
                    SalaryEarn sale = new SalaryEarn();
                    sale.ActionTypeID = cashEarn.ActinTypeID;
                    sale.TotalAmount = amount;
                    sale.Currency = currency;
                    sale.Description = cashEarn.Description;
                    sale.DocumentDate = docDate;
                    sale.EnvironmentID = 2;
                    sale.EmployeeID = fromPrefix == "E" ? fromID : (int?)null;
                    sale.LocationID = cashEarn.LocationID;
                    sale.UID = cashEarn.UID;
                    sale.UnitPrice = unit;
                    sale.QuantityHour = quantity;
                    sale.CategoryID = cashEarn.CategoryID;


                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.EditSalaryEarn(sale, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }

                

            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("Detail", new { id = cashEarn.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteSalaryEarn(string id)
        {
            Result<DocumentSalaryEarn> result = new Result<DocumentSalaryEarn>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeleteSalaryEarn(Guid.Parse(id), model.Authentication);
            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.Message = result.Message;
            messageresult.IsSuccess = result.IsSuccess;

            TempData["result"] = messageresult;
            return RedirectToAction("Detail", new { id = id });

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
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 1 && x.IsActive == true).ToList();
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
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
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
                var amount = Convert.ToDouble(cashSalary.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
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
                    payment.TimeZone = location.Timezone;
                    payment.OurCompanyID = location.OurCompanyID;
                    payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
                    payment.FromBankID = (int?)cashSalary.BankAccountID > 0 ? cashSalary.BankAccountID : (int?)null;
                    payment.FromCashID = (int?)cashSalary.BankAccountID == 0 ? cash.ID : (int?)null;
                    payment.SalaryTypeID = cashSalary.SalaryType;
                    payment.TimeZone = location.Timezone;
                    payment.ReferanceID = cashSalary.ReferanceID;
                    payment.CategoryID = cashSalary.CategoryID ?? (int?)null;

                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddSalaryPayment(payment, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
                
                

            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;

            return RedirectToAction("SalaryPayment", "Salary");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditSalaryPayment(NewCashSalaryPayment cashSalary)
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
                    if (newexchanges > 0)
                    {
                        sale.ExchangeRate = newexchanges;
                    }
                    else
                    {
                        sale.ExchangeRate = exchanges;
                    }

                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.EditSalaryPayment(sale, model.Authentication);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
                
                

            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("SalaryDetail", new { id = cashSalary.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteSalaryPayment(string id)
        {
            Result<DocumentSalaryPayment> result = new Result<DocumentSalaryPayment>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeleteSalaryPayment(Guid.Parse(id), model.Authentication);
            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.IsSuccess = result.IsSuccess;
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("SalaryDetail", new { id = id });

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
            model.SalaryCategories = Db.SalaryCategory.Where(x => x.ParentID == 2 && x.IsActive == true).ToList();
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