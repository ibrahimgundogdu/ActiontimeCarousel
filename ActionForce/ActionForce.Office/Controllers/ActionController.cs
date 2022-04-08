using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ActionController : BaseController
    {
        // GET: Actions
        [AllowAnonymous]
        public ActionResult Index(int? locationId, string dateBegin, string dateEnd)
        {
            ActionControlModel model = new ActionControlModel();

            var DateBegin = DateTime.Now.AddMonths(-1).Date;
            var DateEnd = DateTime.Now.Date;

            if (!string.IsNullOrEmpty(dateBegin))
            {
                DateTime.TryParse(dateBegin, out DateBegin);
            }
            if (!string.IsNullOrEmpty(dateEnd))
            {
                DateTime.TryParse(dateEnd, out DateEnd);
            }


            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as FilterModel;
            }
            else
            {
                FilterModel filterModel = new FilterModel();

                filterModel.LocationID = locationId != null ? locationId : Db.Location.FirstOrDefault(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).LocationID;
                filterModel.DateBegin = DateBegin;
                filterModel.DateEnd = DateEnd;
                model.Filters = filterModel;
            }

            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.ActionList = Db.VCashBankActions.Where(x => x.LocationID == model.Filters.LocationID && x.ActionDate >= model.Filters.DateBegin && x.ActionDate <= model.Filters.DateEnd).OrderBy(x => x.ActionDate).ThenBy(x=> x.RecordDate).ToList();

            var balanceData = Db.VCashBankActions.Where(x => x.LocationID == model.Filters.LocationID && x.ActionDate < model.Filters.DateBegin).ToList();
            if (balanceData != null && balanceData.Count > 0)
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Cash",
                    Total = balanceData.Where(x => x.Currency == "TRL" && x.Module == "Cash").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "TRL" && x.Module == "Bank").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Cash",
                    Total = balanceData.Where(x => x.Currency == "USD" && x.Module == "Cash").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "USD" && x.Module == "Bank").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Cash",
                    Total = balanceData.Where(x => x.Currency == "EUR" && x.Module == "Cash").Sum(x => x.CashAmount) ?? 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = balanceData.Where(x => x.Currency == "EUR" && x.Module == "Bank").Sum(x => x.CashAmount) ?? 0
                });

                model.HeaderTotals = headerTotals;
            }
            else
            {
                List<TotalModel> headerTotals = new List<TotalModel>();

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Cash",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "TRL",
                    Type = "Bank",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Cash",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "USD",
                    Type = "Bank",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Cash",
                    Total = 0
                });

                headerTotals.Add(new TotalModel()
                {
                    Currency = "EUR",
                    Type = "Bank",
                    Total = 0
                });

                model.HeaderTotals = headerTotals;
            }


            List<TotalModel> footerTotals = new List<TotalModel>(); // ilk başta header ile footer aynı olur ekranda foreach içinde footer değişir. 
            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "TRL").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "TRL").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "USD").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "USD",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "USD").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Cash",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Cash" && x.Currency == "EUR").Total
            });

            footerTotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Type = "Bank",
                Total = model.HeaderTotals.FirstOrDefault(x => x.Type == "Bank" && x.Currency == "EUR").Total
            });

            model.FooterTotals = footerTotals;


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

            return RedirectToAction("Index", "Action");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult FilterTransfer(int? locationId, DateTime? beginDate, DateTime? endDate)
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

            return RedirectToAction("Transfers", "Action");
        }

        [AllowAnonymous]
        public ActionResult Transfers()
        {
            ActionControlModel model = new ActionControlModel();

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

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.Transfers = Db.VDocumentTransfer.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.DocumentDate >= model.Filters.DateBegin && x.DocumentDate <= model.Filters.DateEnd).OrderBy(x => x.DocumentDate).ToList();

            if (model.CurrentLocation != null)
            {
                model.Transfers = model.Transfers.Where(x => x.FromLocationID == model.CurrentLocation.LocationID).OrderBy(x => x.DocumentDate).ToList();
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult AddTransfer()
        {
            ActionControlModel model = new ActionControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<DocumentTransfer> ?? null;
            }


            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            List<int> locationids = model.LocationList.Select(z => z.LocationID).ToList();
            model.CashList = Db.VCash.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrencyList = Db.Currency.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddNewTransfer(newTransfer transfer)
        {
            Result<DocumentTransfer> result = new Result<DocumentTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ActionControlModel model = new ActionControlModel();
            var amount = Convert.ToDouble(transfer.Amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var date = Convert.ToDateTime(transfer.DocumentDate);
            string uid = string.Empty;

            if (transfer != null && amount >0)
            {
                DocumentManager manager = new DocumentManager();

                TransferModel transfermodel = new TransferModel();

                transfermodel.Amount = amount;
                transfermodel.CarrierEmployeeID = transfer.CarrierEmployeeID;
                transfermodel.Currency = transfer.Currency;
                transfermodel.Description = transfer.Description;
                transfermodel.DocumentDate = date;
                transfermodel.FromBankID = transfer.FromBankID;
                transfermodel.FromCashID = transfer.FromCashID;
                transfermodel.FromCustID = transfer.FromCustID;
                transfermodel.FromEmplID = transfer.FromEmplID;
                transfermodel.FromLocationID = transfer.FromLocationID;
                transfermodel.ToBankID = transfer.ToBankID;
                transfermodel.ToCashID = transfer.ToCashID;
                transfermodel.ToCustID = transfer.ToCustID;
                transfermodel.ToEmplID = transfer.ToEmplID;
                transfermodel.ToLocationID = transfer.ToLocationID;
                transfermodel.UID = Guid.NewGuid();

                uid = transfermodel.UID.ToString();

                result = manager.AddTransfer(transfermodel, model.Authentication);
            }
            else
            {
                result.Message = "Tutar 0 dan büyük olduğuna emin olun";
            }

            TempData["result"] = result;

            if (result.IsSuccess == true)
            {
                return RedirectToAction("TransferDetail","Action", new { id = uid });
            }
            else
            {
                return RedirectToAction("AddTransfer", "Action");
            }
            
        }

        [AllowAnonymous]
        public ActionResult TransferDetail(Guid id)
        {
            ActionControlModel model = new ActionControlModel();

            if (TempData["result"] != null)
            {
                if (TempData["result"] != null)
                {
                    model.Result = TempData["result"] as Result<DocumentTransfer> ?? null;
                }
            }

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            List<int> locationids = model.LocationList.Select(z => z.LocationID).ToList();
            model.CashList = Db.VCash.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            model.CurrencyList = Db.Currency.ToList();
            model.CurrentTransfer = Db.VDocumentTransfer.FirstOrDefault(x => x.UID == id);
            model.TransferStatus = Db.DocumentTransferStatus.ToList();
            model.LogList = Db.ApplicationLog.Where(x => x.Modul == "Transfer" && x.ProcessID == model.CurrentTransfer.ID.ToString()).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditTransfer(editTransfer transfer)
        {
            Result<DocumentTransfer> result = new Result<DocumentTransfer>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ActionControlModel model = new ActionControlModel();

            var amount = Convert.ToDouble(transfer.Amount?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var exchangerate = Convert.ToDouble(transfer.ExchangeRate?.ToString().Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            var date = Convert.ToDateTime(transfer.DocumentDate);
            bool isactive = false;

            if (!string.IsNullOrEmpty(transfer.IsActive) && transfer.IsActive == "1")
            {
                isactive = true;
            }

            if (transfer != null && amount > 0)
            {
                DocumentManager manager = new DocumentManager();

                TransferModel transfermodel = new TransferModel();

                transfermodel.Amount = amount;
                transfermodel.CarrierEmployeeID = transfer.CarrierEmployeeID;
                transfermodel.Currency = transfer.Currency;
                transfermodel.Description = transfer.Description;
                transfermodel.DocumentDate = date;
                transfermodel.FromBankID = transfer.FromBankID;
                transfermodel.FromCashID = transfer.FromCashID;
                transfermodel.FromCustID = transfer.FromCustID;
                transfermodel.FromEmplID = transfer.FromEmplID;
                transfermodel.FromLocationID = transfer.FromLocationID;
                transfermodel.ToBankID = transfer.ToBankID;
                transfermodel.ToCashID = transfer.ToCashID;
                transfermodel.ToCustID = transfer.ToCustID;
                transfermodel.ToEmplID = transfer.ToEmplID;
                transfermodel.ToLocationID = transfer.ToLocationID;
                transfermodel.UID = transfer.UID.Value;
                transfermodel.ID = transfer.ID;
                transfermodel.ExchangeRate = exchangerate;
                transfermodel.IsActive = isactive;
                transfermodel.StatusID = transfer.StatusID.Value;


                result = manager.EditTransfer(transfermodel, model.Authentication);
            }
            else
            {
                result.Message = "Tutar 0 dan büyük olduğuna emin olun";
            }

            TempData["result"] = result;

            return RedirectToAction("TransferDetail", new { id = transfer.UID });
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationCashSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CashList = Db.VCash.Where(x => x.LocationID == locationid).ToList();

            return PartialView("_PartialSelectLocationCash", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationBankSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectLocationBank", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationEmployeeSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectLocationEmployee", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult FLocationCustomerSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CustomerList = Db.Customer.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectLocationCustomer", model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationCashSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CashList = Db.VCash.Where(x => x.LocationID == locationid).ToList();

            return PartialView("_PartialSelectTLocationCash", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationBankSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectTLocationBank", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationEmployeeSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.EmployeeList = Db.Employee.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectTLocationEmployee", model);
        }

        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult TLocationCustomerSelect(int? locationid, string date)
        {
            ActionControlModel model = new ActionControlModel();

            model.CurrentDate = Convert.ToDateTime(date);
            model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == locationid.Value);
            model.CustomerList = Db.Customer.Where(x => x.OurCompanyID == model.CurrentLocation.OurCompanyID).ToList();

            return PartialView("_PartialSelectTLocationCustomer", model);
        }


        //Expense
        [AllowAnonymous]
        public ActionResult Expense(int? ECID, int? EIID, int? EGID, int? ESID, int? EDID, string EPCD)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as ExpenseFilterModel;
                model.Filters.FromSearch = true;
            }
            else
            {
                ExpenseFilterModel filterModel = new ExpenseFilterModel();

                filterModel.ExpenseCenterID = ECID ?? null;
                filterModel.ExpenseItemID = EIID ?? null;
                filterModel.ExpenseGroupID = EGID ?? null;
                filterModel.DistributeGroupID = EDID ?? null;
                filterModel.ExpenseStatusID = ESID ?? null;
                filterModel.ExpensePeriodCode = !string.IsNullOrEmpty(EPCD) ? EPCD : string.Empty;
                filterModel.DateBegin = new DateTime(DateTime.Now.Year, 1, 1);
                filterModel.DateEnd = DateTime.Now.Date;
                filterModel.FromSearch = false;
                model.Filters = filterModel;
            }

            model.ExpenseCenters = Db.ExpenseCenter.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseDocumentStatuses = Db.ExpenseDocumentStatus.OrderBy(x => x.SortBy).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.OrderBy(x => x.SortBy).ToList();
            model.ExpenseChartGroups = Db.ExpenseChartGroup.OrderBy(x => x.SortBy).ToList();

            IQueryable<VExpenseDocument> expenseDocuments;

            if (model.Filters.FromSearch == true || ECID != null || EIID != null || EGID != null || ESID != null || EDID != null || !string.IsNullOrEmpty(EPCD))
            {
                expenseDocuments = Db.VExpenseDocument.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.RecordEmployeeID == model.Authentication.ActionEmployee.EmployeeID);

                if (model.Filters.ExpenseCenterID != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.ExpenseCenterID == model.Filters.ExpenseCenterID);
                }

                if (model.Filters.ExpenseItemID != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.ExpenseItemID == model.Filters.ExpenseItemID);
                }

                if (model.Filters.ExpenseGroupID != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.ExpenseGroupID == model.Filters.ExpenseGroupID);
                }

                if (model.Filters.DistributeGroupID != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.DistributeGroupID == model.Filters.DistributeGroupID);
                }

                if (model.Filters.ExpenseStatusID != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.StatusID == model.Filters.ExpenseStatusID);
                }

                if (!string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
                {
                    expenseDocuments = expenseDocuments.Where(x => x.ExpensePeriodCode == model.Filters.ExpensePeriodCode);
                }

                if (model.Filters.DateBegin != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.DocumentDate >= model.Filters.DateBegin);
                }

                if (model.Filters.DateEnd != null)
                {
                    expenseDocuments = expenseDocuments.Where(x => x.DocumentDate <= model.Filters.DateEnd);
                }

                model.ExpenseDocuments = expenseDocuments.ToList();
            }

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExpenseFilter(int? ECID, int? EIID, int? EGID, int? ESID, int? EDID, string EPCD, DateTime? DTBG, DateTime DTEN)
        {
            ExpenseFilterModel model = new ExpenseFilterModel();

            model.ExpenseCenterID = ECID ?? null;
            model.ExpenseItemID = EIID ?? null;
            model.ExpenseGroupID = EGID ?? null;
            model.DistributeGroupID = EDID ?? null;
            model.ExpenseStatusID = ESID ?? null;
            model.ExpensePeriodCode = !string.IsNullOrEmpty(EPCD) ? EPCD : string.Empty;
            model.DateBegin = DTBG != null ? DTBG : new DateTime(DateTime.Now.Year, 1, 1);
            model.DateEnd = DTEN != null ? DTEN : DateTime.Now.Date;
            model.FromSearch = true;

            if (DTBG == null)
            {
                DateTime begin = DateTime.Now.Date;
                model.DateBegin = new DateTime(begin.Year, 1, 1);
            }

            if (DTEN == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }


            TempData["filter"] = model;

            return RedirectToAction("Expense", "Action");
        }


        [AllowAnonymous]
        public ActionResult NewDocument()
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.ExpenseCenters = Db.ExpenseCenter.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseDocumentStatuses = Db.ExpenseDocumentStatus.OrderBy(x => x.SortBy).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddExpense(ExpenseFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var totalAmount = Convert.ToDouble(form.TotalAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var distAmount = Convert.ToDouble(form.DistributionAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var taxRate = Convert.ToDouble(form.TaxRate.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                var document = Db.ExpenseDocument.FirstOrDefault(x => x.ExpenseCenterID == form.ExpenseCenterID && x.ExpenseItemID == form.ExpenseItemID && x.ExpenseGroupID == form.ExpenseGroupID && x.TotalAmount == totalAmount);

                if (document == null)
                {
                    document = new ExpenseDocument();
                    form.UID = Guid.NewGuid();

                    document.UID = form.UID;
                    document.DocumentNumber = OfficeHelper.GetDocumentNumber(model.Authentication.ActionEmployee.OurCompanyID ?? 2, "EXD");
                    document.RecordDate = DateTime.UtcNow.AddHours(3);
                    document.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    document.RecordIP = OfficeHelper.GetIPAddress();
                    document.DocumentSource = form.DocumentSource;
                    document.ExpenseDescription = form.ExpenseDescription;
                    document.DistributionAmount = distAmount;
                    document.TotalAmount = totalAmount;
                    document.ExpenseGroupID = form.ExpenseGroupID;
                    document.Currency = form.Currency;
                    document.DocumentDate = form.DocumentDate;
                    document.ExpenseItemID = form.ExpenseItemID;
                    document.ExpenseCenterID = form.ExpenseCenterID;
                    document.ExpensePeriod = form.ExpensePeriod;
                    document.IsActive = true;
                    document.StatusID = 0;
                    document.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID;
                    document.TaxRate = taxRate;
                    document.DistributeGroupID = 5;

                    if (form.ExpensePeriod != null)
                    {
                        document.ExpenseYear = document.ExpensePeriod.Value.Year;
                        document.ExpenseMonth = document.ExpensePeriod.Value.Month;
                        document.ExpensePeriodCode = document.ExpenseYear.ToString() + "-" + (document.ExpenseMonth <= 9 ? "0" + document.ExpenseMonth.ToString() : document.ExpenseMonth.ToString());
                    }

                    Db.ExpenseDocument.Add(document);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Dokümanı Eklendi";

                    OfficeHelper.AddApplicationLog("Office", "ExpenseDocument", "Insert", document.ID.ToString(), "Expense", "NewDocument", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, document);
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Benzer Masraf Dokümanı Bulundu";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Action", new { id = form.UID });
        }


        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Expense");
            }

            model.ExpenseCenters = Db.ExpenseCenter.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseDocumentStatuses = Db.ExpenseDocumentStatus.OrderBy(x => x.SortBy).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.ToList();
            model.ExpenseChartGroups = Db.ExpenseChartGroup.ToList();
            model.ExpenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (model.ExpenseDocument == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Expense");
            }

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditExpense(ExpenseFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var document = Db.ExpenseDocument.FirstOrDefault(x => x.ID == form.ExpenseDocumentID);

                if (document != null)
                {
                    ExpenseDocument self = new ExpenseDocument()
                    {
                        RecordDate = document.RecordDate,
                        RecordEmployeeID = document.RecordEmployeeID,
                        RecordIP = document.RecordIP,
                        IsActive = document.IsActive,
                        OurCompanyID = document.OurCompanyID,
                        Currency = document.Currency,
                        UID = document.UID,
                        DocumentNumber = document.DocumentNumber,
                        DistributionAmount = document.DistributionAmount,
                        DocumentDate = document.DocumentDate,
                        DocumentSource = document.DocumentSource,
                        ExpenseCenterID = document.ExpenseCenterID,
                        ExpenseDescription = document.ExpenseDescription,
                        ExpenseGroupID = document.ExpenseGroupID,
                        ExpenseItemID = document.ExpenseItemID,
                        ExpenseMonth = document.ExpenseMonth,
                        ExpensePeriod = document.ExpensePeriod,
                        ExpensePeriodCode = document.ExpensePeriodCode,
                        ExpenseYear = document.ExpenseYear,
                        ID = document.ID,
                        StatusID = document.StatusID,
                        TotalAmount = document.TotalAmount,
                        TaxRate = document.TaxRate,
                        AutoComputeTypeID = document.AutoComputeTypeID,
                        TaxAmount = document.TaxAmount
                    };

                    var totalAmount = Convert.ToDouble(form.TotalAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    var distAmount = Convert.ToDouble(form.DistributionAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    var taxRate = Convert.ToDouble(form.TaxRate.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                    document.ExpenseDescription = form.ExpenseDescription;

                    if (document.StatusID == 0)
                    {
                        document.DistributionAmount = distAmount;
                        document.TotalAmount = totalAmount;
                        document.TaxRate = taxRate;
                        document.ExpenseGroupID = form.ExpenseGroupID;
                        document.Currency = form.Currency;
                        document.DocumentDate = form.DocumentDate;
                        document.ExpenseItemID = form.ExpenseItemID;
                        document.ExpenseCenterID = form.ExpenseCenterID;
                        document.ExpensePeriod = form.ExpensePeriod;

                        if (form.ExpensePeriod != null)
                        {
                            document.ExpenseYear = document.ExpensePeriod.Value.Year;
                            document.ExpenseMonth = document.ExpensePeriod.Value.Month;
                            document.ExpensePeriodCode = document.ExpenseYear.ToString() + "-" + (document.ExpenseMonth <= 9 ? "0" + document.ExpenseMonth.ToString() : document.ExpenseMonth.ToString());
                        }
                    }

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Dokümanı Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<ExpenseDocument>(self, document, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "ExpenseDocument", "Update", document.ID.ToString(), "Expense", "Detail", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Masraf Dokümanı Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Detail", "Action", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult Chart(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Expense");
            }

            model.ExpenseCenters = Db.ExpenseCenter.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true && x.ProfitCenter == true).OrderBy(x => x.SortBy).ToList();
            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseDocumentStatuses = Db.ExpenseDocumentStatus.OrderBy(x => x.SortBy).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.ToList();
            model.ExpenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == id);
            model.ExpenseChartGroups = Db.ExpenseChartGroup.Where(x => x.IsActive == true && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (model.ExpenseDocument == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Expense");
            }

            DocumentManager manager = new DocumentManager();
            model.ExpenseDocumentCharts = manager.GetExpenseDucumentChart(model.ExpenseDocument, model.Authentication);

            List<int> expCenterIds = model.ExpenseDocumentCharts.Select(x => x.ExpenseCenterID.Value).Distinct().ToList();
            model.ExpenseCenters = model.ExpenseCenters.Where(x => !expCenterIds.Contains(x.ID)).ToList();

            return View(model);

        }

        [AllowAnonymous]
        public ActionResult Remove(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();


            var expenseDocument = Db.ExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (expenseDocument != null)
            {
                var cartItems = Db.ExpenseDocumentChart.Where(x => x.ExpenseDocumentID == expenseDocument.ID).ToList();
                var rowItems = Db.ExpenseDocumentRows.Where(x => x.DocumentID == expenseDocument.ID).ToList();
                var actionItems = Db.ExpenseActions.Where(x => x.DocumentID == expenseDocument.ID).ToList();

                model.Result.IsSuccess = true;
                model.Result.Message = "Masraf Dokümanı listeden silindi";

                OfficeHelper.AddApplicationLog("Office", "ExpenseDocument", "Delete", expenseDocument.ID.ToString(), "Expense", "Remove", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, expenseDocument);

                Db.ExpenseDocumentChart.RemoveRange(cartItems);
                Db.ExpenseDocumentRows.RemoveRange(rowItems);
                Db.ExpenseActions.RemoveRange(actionItems);
                Db.ExpenseDocument.Remove(expenseDocument);

                Db.SaveChanges();
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Masraf Dokümanı Bulunamadı!";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Expense", "Action", new { id });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddExpenseCenter(Guid? EUID, int? ECID)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (EUID != null && ECID != null)
            {
                var document = Db.ExpenseDocument.FirstOrDefault(x => x.UID == EUID);

                if (document != null)
                {
                    ExpenseDocumentChart chart = new ExpenseDocumentChart();

                    chart.Currency = document.Currency;
                    chart.DistributedAmount = 0;
                    chart.DistributedRate = 0;
                    chart.DistributionAmount = document.DistributionAmount;
                    chart.ExpenseCenterID = ECID;
                    chart.ExpenseDocumentID = document.ID;
                    chart.ExpenseItemID = document.ExpenseItemID;
                    chart.RecordDate = DateTime.UtcNow.AddHours(3);
                    chart.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    chart.RecordIP = OfficeHelper.GetIPAddress();
                    chart.UID = Guid.NewGuid();

                    Db.ExpenseDocumentChart.Add(chart);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Dağılım Cetveline Merkez Eklendi";

                    OfficeHelper.AddApplicationLog("Office", "ExpenseDocumentChart", "Insert", document.ID.ToString(), "Expense", "AddExpenseCenter", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, chart);
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Masraf Dokümanı Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Chart", "Action", new { id = EUID });
        }

        [AllowAnonymous]
        public ActionResult DistributeEQ(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Expense");
            }

            model.ExpenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == id);
            var parentDocument = Db.ExpenseDocument.FirstOrDefault(x => x.ExpenseCenterID == model.ExpenseDocument.ExpenseCenterID && x.ExpenseGroupID == model.ExpenseDocument.ExpenseGroupID && x.ExpensePeriodCode == model.ExpenseDocument.ExpensePeriodCode && x.ExpenseItemID == 5);

            if (model.ExpenseDocument == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Expense");
            }


            if (model.ExpenseDocument.AutoComputeTypeID > 0)
            {
                if (model.ExpenseDocument.AutoComputeTypeID == 1)
                {
                    Db.AddExpenseDocumentChart(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 2)
                {
                    Db.AddExpenseDocumentChartPremium(model.ExpenseDocument.ID, parentDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 3)
                {
                    Db.AddExpenseDocumentChartSGK(model.ExpenseDocument.ID, parentDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 4)
                {
                    Db.AddExpenseDocumentChartFoodCard(model.ExpenseDocument.ID, parentDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }


                if (model.ExpenseDocument.AutoComputeTypeID == 5)
                {
                    Db.AddExpenseDocumentChartMonthlySalary(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 6)
                {
                    Db.AddExpenseDocumentChartMonthlyPremium(model.ExpenseDocument.ID, parentDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 7)
                {
                    Db.AddExpenseDocumentChartMonthlySGK(model.ExpenseDocument.ID, parentDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 8)
                {
                    Db.AddExpenseDocumentChartMonthlyFoodCard(model.ExpenseDocument.ID, parentDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 9)
                {
                    Db.AddExpenseDocumentChart(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 10)
                {
                    Db.AddExpenseDocumentChartOfficeRent(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 11)
                {
                    Db.AddExpenseDocumentChart(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 12)
                {
                    Db.AddExpenseDocumentChart(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }

                if (model.ExpenseDocument.AutoComputeTypeID == 13)
                {
                    Db.AddExpenseDocumentChartKidem(model.ExpenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress());
                }




            }
            else
            {
                DocumentManager manager = new DocumentManager();
                model.ExpenseDocumentCharts = manager.GetExpenseDucumentChart(model.ExpenseDocument, model.Authentication);
                var charts = Db.ExpenseDocumentChart.Where(x => x.ExpenseDocumentID == model.ExpenseDocument.ID).ToList();

                foreach (var item in charts)
                {
                    item.DistributionAmount = model.ExpenseDocument.DistributionAmount;
                    item.DistributedAmount = (item.DistributionAmount / (double)charts.Count);
                    item.DistributedRate = (1 / (double)charts.Count) * 100;
                    item.UpdateDate = DateTime.UtcNow.AddHours(3);
                    item.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    item.UpdateIP = OfficeHelper.GetIPAddress();
                }

                Db.SaveChanges();
            }




            return RedirectToAction("Chart", "Action", new { id });

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ArrangeByGroup(ArrangeFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            if (form.ECGID == null || form.EUID == null)
            {
                return RedirectToAction("Expense");
            }

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.ExpenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == form.EUID);

            if (model.ExpenseDocument == null)
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Masraf dokümanı bulunamadı";

                TempData["Result"] = model.Result;
                return RedirectToAction("Expense");
            }


            if (model.ExpenseDocument.AutoComputeTypeID <= 0 || model.ExpenseDocument.AutoComputeTypeID == null)
            {

                var sresult = Db.AddExpenseDocumentGroupRowsChart(model.ExpenseDocument.ID, form.ECGID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress()).FirstOrDefault().Value;

                if (sresult > 0)
                {
                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Dokümanında Dağılım Cetveli Oluşturuldu";
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Manuel eklenen bir doküman olmalı. Otomatik dağılım dosyalarına uygulanamaz";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Chart", "Action", new { id = model.ExpenseDocument.UID });

        }

        [AllowAnonymous]
        public ActionResult RemoveCost(Guid? id, long cid)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();


            var expenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == id);
            if (expenseDocument != null)
            {
                var costItem = Db.ExpenseDocumentChart.FirstOrDefault(x => x.ID == cid && x.ExpenseDocumentID == expenseDocument.ID);

                model.Result.IsSuccess = true;
                model.Result.Message = "Masraf Merkezi dağılımı listeden silindi";

                OfficeHelper.AddApplicationLog("Office", "ExpenseDocument", "Delete", costItem.ID.ToString(), "Expense", "RemoveCost", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, costItem);

                Db.ExpenseDocumentChart.Remove(costItem);
                Db.SaveChanges();
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Masraf Dokümanı Bulunamadı!";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Chart", "Action", new { id });
        }

        [AllowAnonymous]
        public ActionResult CreateCost(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            List<ExpenseActions> expenseActionsList = new List<ExpenseActions>();

            if (id == null)
            {
                return RedirectToAction("Expense");
            }

            var expenseDocument = Db.ExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (expenseDocument == null)
            {
                model.Result.Message = "Masraf Dokümanı Bulunamadı";

                TempData["Result"] = model.Result;
                return RedirectToAction("Expense");
            }

            var sresult = Db.AddExpenseDocumentToActions(expenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress()).FirstOrDefault();

            model.Result.IsSuccess = true;
            model.Result.Message = "Maliyet Satırları Oluşturuldu";

            TempData["Result"] = model.Result;

            OfficeHelper.AddApplicationLog("Office", "ExpenseActions", "Insert", expenseDocument.ID.ToString(), "Expense", "CreateCost", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

            return RedirectToAction("Chart", "Action", new { id });

        }

        [AllowAnonymous]
        public ActionResult ResetCost(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            if (id == null)
            {
                return RedirectToAction("Expense");
            }

            var expenseDocument = Db.ExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (expenseDocument == null)
            {
                model.Result.Message = "Masraf Dokümanı Bulunamadı";

                TempData["Result"] = model.Result;
                return RedirectToAction("Expense");
            }

            var actions = Db.ExpenseActions.Where(x => x.DocumentID == expenseDocument.ID && x.ReferenceUID == expenseDocument.UID).ToList();

            Db.ExpenseActions.RemoveRange(actions);
            Db.SaveChanges();

            expenseDocument.StatusID = 0;
            Db.SaveChanges();

            model.Result.IsSuccess = true;
            model.Result.Message = "Maliyet İşlemleri Kaldırıldı";

            TempData["Result"] = model.Result;

            OfficeHelper.AddApplicationLog("Office", "ExpenseActions", "Delete", expenseDocument.ID.ToString(), "Expense", "ResetCost", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

            return RedirectToAction("Chart", "Action", new { id });

        }






    }
}