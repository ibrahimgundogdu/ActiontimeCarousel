using ActionForce.Entity;
using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ExpenseController : BaseController
    {

        [AllowAnonymous]
        public ActionResult Index(int? ECID, int? EIID, int? EGID, int? ESID, int? EDID, string EPCD)
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
                expenseDocuments = Db.VExpenseDocument.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

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

            return RedirectToAction("Index", "Expense");
        }



        [AllowAnonymous]
        public ActionResult Cost(int? ECID, int? EIID, int? EGID, int? ESID, string EPCD)
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
                filterModel.ExpensePeriodCode = !string.IsNullOrEmpty(EPCD) ? EPCD : string.Empty;
                filterModel.FromSearch = false;
                model.Filters = filterModel;

            }

            model.ExpenseCenters = Db.ExpenseCenter.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ProfitCenter == true && x.ExpenseCenter1 == true).OrderBy(x => x.SortBy).ToList();
            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.OrderBy(x => x.SortBy).ToList();

            IQueryable<VExpenseActions> expenseActions;

            if (model.Filters.FromSearch == true || ECID != null || EIID != null || EGID != null || !string.IsNullOrEmpty(EPCD))
            {
                expenseActions = Db.VExpenseActions.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.ExpenseCenterID != null)
                {
                    expenseActions = expenseActions.Where(x => x.LocationID == model.Filters.ExpenseCenterID);
                }

                if (model.Filters.ExpenseItemID != null)
                {
                    expenseActions = expenseActions.Where(x => x.ExpenseItemID == model.Filters.ExpenseItemID);
                }

                if (model.Filters.ExpenseGroupID != null)
                {
                    expenseActions = expenseActions.Where(x => x.ExpenseGroupID == model.Filters.ExpenseGroupID);
                }

                if (!string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
                {
                    expenseActions = expenseActions.Where(x => x.ExpensePeriodCode == model.Filters.ExpensePeriodCode);
                }

                model.ExpenseActions = expenseActions.ToList();
            }

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult CostFilter(int? ECID, int? EIID, int? EGID, string EPCD)
        {
            ExpenseFilterModel model = new ExpenseFilterModel();

            model.ExpenseCenterID = ECID ?? null;
            model.ExpenseItemID = EIID ?? null;
            model.ExpenseGroupID = EGID ?? null;
            model.ExpensePeriodCode = !string.IsNullOrEmpty(EPCD) ? EPCD : string.Empty;
            model.FromSearch = true;

            TempData["filter"] = model;

            return RedirectToAction("Cost", "Expense");
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

            return RedirectToAction("Detail", "Expense", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult NewItem(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id != null)
            {
                model.ExpenseItem = Db.ExpenseItem.FirstOrDefault(x => x.UID == id);
                model.ItemEditable = !Db.ExpenseDocument.Any(x => x.ExpenseItemID == model.ExpenseItem.ID);

            }
            else
            {
                model.ExpenseItem = null;
            }

            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.ToList();
            model.OurCompanies = Db.OurCompany.ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddExpenseItem(ExpenseItemFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (form != null)
            {


                var document = Db.ExpenseItem.FirstOrDefault(x => x.ExpenseItemName == form.ExpenseItemName);

                if (document == null)
                {
                    document = new ExpenseItem();
                    form.UID = Guid.NewGuid();

                    document.UID = form.UID;
                    document.ExpenseItemName = form.ExpenseItemName.Trim();
                    document.SortBy = form.SortBy;
                    document.IsLocation = form.IsLocation == "1" ? true : false;
                    document.IsGeneral = form.IsGeneral == "1" ? true : false;
                    document.IsOffice = form.IsOffice == "1" ? true : false;
                    document.IsActive = true;
                    document.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID;


                    Db.ExpenseItem.Add(document);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Kalemi Eklendi";

                    OfficeHelper.AddApplicationLog("Office", "ExpenseItem", "Insert", document.ID.ToString(), "Expense", "AddExpenseItem", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, document);
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Benzer Masraf Kalemi Bulundu";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("NewItem", "Expense", new { id = form.UID });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditExpenseItem(ExpenseItemFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var document = Db.ExpenseItem.FirstOrDefault(x => x.UID == form.UID);

                if (document != null)
                {
                    ExpenseItem self = new ExpenseItem()
                    {

                        IsActive = document.IsActive,
                        OurCompanyID = document.OurCompanyID,
                        UID = document.UID,
                        ID = document.ID,
                        ExpenseItemName = document.ExpenseItemName,
                        SortBy = document.SortBy,
                        IsLocation = document.IsLocation,
                        IsGeneral = document.IsGeneral,
                        IsOffice = document.IsOffice

                    };

                    document.OurCompanyID = form.OurCompanyID;
                    document.ExpenseItemName = form.ExpenseItemName;
                    document.SortBy = form.SortBy;
                    document.IsLocation = form.IsLocation == "1" ? true : false;
                    document.IsGeneral = form.IsGeneral == "1" ? true : false;
                    document.IsOffice = form.IsOffice == "1" ? true : false;
                    document.IsActive = form.IsActive == "1" ? true : false;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Kalemi Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<ExpenseItem>(self, document, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "ExpenseItem", "Update", document.ID.ToString(), "Expense", "EditExpenseItem", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);


                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Masraf Kalemi Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("NewItem", "Expense", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult ItemCheck(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            var document = Db.ExpenseItem.FirstOrDefault(x => x.UID == id);

            if (document != null)
            {
                document.IsActive = !document.IsActive;
                Db.SaveChanges();
            }

            return RedirectToAction("NewItem", "Expense", new { id });
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
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
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

            return RedirectToAction("Detail", "Expense", new { id = form.UID });
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
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
            }

            DocumentManager manager = new DocumentManager();
            model.ExpenseDocumentCharts = manager.GetExpenseDucumentChart(model.ExpenseDocument, model.Authentication);

            List<int> expCenterIds = model.ExpenseDocumentCharts.Select(x => x.ExpenseCenterID.Value).Distinct().ToList();
            model.ExpenseCenters = model.ExpenseCenters.Where(x => !expCenterIds.Contains(x.ID)).ToList();

            return View(model);

        }


        [AllowAnonymous]
        public ActionResult Rows(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            model.ExpenseCenters = Db.ExpenseCenter.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true && x.ProfitCenter == true).OrderBy(x => x.SortBy).ToList();
            model.ExpenseItems = Db.ExpenseItem.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).OrderBy(x => x.SortBy).ToList();
            model.ExpenseDocumentStatuses = Db.ExpenseDocumentStatus.OrderBy(x => x.SortBy).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.ExpenseGroups = Db.ExpenseGroup.ToList();
            model.ExpenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (model.ExpenseDocument == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Index");
            }

            DocumentManager manager = new DocumentManager();
            model.ExpenseDocumentRows = Db.VExpenseDocumentRows.Where(x => x.DocumentID == model.ExpenseDocument.ID).ToList();

            return View(model);

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
                return RedirectToAction("Index");
            }

            model.ExpenseDocument = Db.VExpenseDocument.FirstOrDefault(x => x.UID == id);
            var parentDocument = Db.ExpenseDocument.FirstOrDefault(x => x.ExpenseCenterID == model.ExpenseDocument.ExpenseCenterID && x.ExpenseGroupID == model.ExpenseDocument.ExpenseGroupID && x.ExpensePeriodCode == model.ExpenseDocument.ExpensePeriodCode && x.ExpenseItemID == 5);

            if (model.ExpenseDocument == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Index");
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




            return RedirectToAction("Chart", "Expense", new { id });

        }

        //RemoveCost
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

            return RedirectToAction("Chart", "Expense", new { id });
        }

        [AllowAnonymous]
        public JsonResult SetExpenseChart(long id, float amount)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            ExpenseChartUpdateModel jmodel = new ExpenseChartUpdateModel();

            var expenseDocumentChart = Db.ExpenseDocumentChart.FirstOrDefault(x => x.ID == id);

            if (expenseDocumentChart != null) //&& amount > 0
            {
                expenseDocumentChart.DistributedAmount = amount;
                expenseDocumentChart.DistributedRate = (expenseDocumentChart.DistributedAmount / expenseDocumentChart.DistributionAmount) * 100;
                expenseDocumentChart.UpdateDate = DateTime.UtcNow.AddHours(3);
                expenseDocumentChart.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                expenseDocumentChart.UpdateIP = OfficeHelper.GetIPAddress();
                Db.SaveChanges();
            }

            var expenseDocumentCharts = Db.ExpenseDocumentChart.Where(x => x.ExpenseDocumentID == expenseDocumentChart.ExpenseDocumentID).ToList();

            jmodel.ID = id;
            jmodel.Rate = expenseDocumentChart.DistributedRate;
            jmodel.Amount = expenseDocumentChart.DistributedAmount;
            jmodel.UpdateDate = expenseDocumentChart.UpdateDate.ToString();
            jmodel.UpdateEmployee = model.Authentication.ActionEmployee.FullName;
            jmodel.DistAmount = expenseDocumentCharts.Sum(x => x.DistributedAmount)?.ToString("N4");
            jmodel.DistRate = expenseDocumentCharts.Sum(x => x.DistributedRate)?.ToString("N4");
            jmodel.BalanceAmount = (expenseDocumentChart.DistributionAmount - expenseDocumentCharts.Sum(x => x.DistributedAmount))?.ToString("N4");
            jmodel.BalanceRate = (100 - expenseDocumentCharts.Sum(x => x.DistributedRate))?.ToString("N4");
            jmodel.ShowButton = (expenseDocumentChart.DistributionAmount - expenseDocumentCharts.Sum(x => x.DistributedAmount)) < 0.1 && (expenseDocumentChart.DistributionAmount - expenseDocumentCharts.Sum(x => x.DistributedAmount)) > -0.1 ? 1 : 0;

            return Json(jmodel, JsonRequestBehavior.AllowGet);

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

            return RedirectToAction("Chart", "Expense", new { id = EUID });
        }

        [AllowAnonymous]
        public ActionResult CreateCost(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            List<ExpenseActions> expenseActionsList = new List<ExpenseActions>();

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var expenseDocument = Db.ExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (expenseDocument == null)
            {
                model.Result.Message = "Masraf Dokümanı Bulunamadı";

                TempData["Result"] = model.Result;
                return RedirectToAction("Index");
            }

            //var charts = Db.ExpenseDocumentChart.Where(x => x.ExpenseDocumentID == expenseDocument.ID).ToList();

            //foreach (var item in charts)
            //{
            //    ExpenseActions exa = new ExpenseActions();

            //    exa.Amount = item.DistributedAmount;
            //    exa.Currency = item.Currency;
            //    exa.DocumentID = item.ExpenseDocumentID;
            //    exa.DocumentNumber = expenseDocument.DocumentNumber;
            //    exa.ExpenseCenterID = (short)expenseDocument.ExpenseCenterID;
            //    exa.ExpenseDescription = expenseDocument.ExpenseDescription;
            //    exa.ExpenseGroupID = expenseDocument.ExpenseGroupID;
            //    exa.ExpenseItemID = item.ExpenseItemID;
            //    exa.ExpenseMonth = expenseDocument.ExpenseMonth;
            //    exa.ExpensePeriod = expenseDocument.ExpensePeriod;
            //    exa.ExpensePeriodCode = expenseDocument.ExpensePeriodCode;
            //    exa.ExpenseYear = expenseDocument.ExpenseYear;
            //    exa.LocationID = (short)item.ExpenseCenterID;
            //    exa.OurCompanyID = expenseDocument.OurCompanyID;
            //    exa.Rate = item.DistributedRate;
            //    exa.RecordDate = DateTime.UtcNow.AddHours(3);
            //    exa.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
            //    exa.RecordIP = OfficeHelper.GetIPAddress();
            //    exa.ReferenceUID = expenseDocument.UID;
            //    exa.Total = item.DistributionAmount;

            //    expenseActionsList.Add(exa);
            //}

            //Db.ExpenseActions.AddRange(expenseActionsList);
            //Db.SaveChanges();

            //expenseDocument.StatusID = 1;
            //Db.SaveChanges();

            var sresult = Db.AddExpenseDocumentToActions(expenseDocument.ID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress()).FirstOrDefault();

            model.Result.IsSuccess = true;
            model.Result.Message = "Maliyet Satırları Oluşturuldu";

            TempData["Result"] = model.Result;

            OfficeHelper.AddApplicationLog("Office", "ExpenseActions", "Insert", expenseDocument.ID.ToString(), "Expense", "CreateCost", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

            return RedirectToAction("Chart", "Expense", new { id });

        }

        [AllowAnonymous]
        public ActionResult ResetCost(Guid? id)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var expenseDocument = Db.ExpenseDocument.FirstOrDefault(x => x.UID == id);

            if (expenseDocument == null)
            {
                model.Result.Message = "Masraf Dokümanı Bulunamadı";

                TempData["Result"] = model.Result;
                return RedirectToAction("Index");
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

            return RedirectToAction("Chart", "Expense", new { id });

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddExpenseAuto(string ExpensePeriod, string Location, string Office, string Rent, string Vat, string Expense, string Reset, string Manuel)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            DocumentManager documentManager = new DocumentManager();


            if (!string.IsNullOrEmpty(ExpensePeriod))
            {
                if (Location == "1")
                {
                    var document = documentManager.ComputeExpenseDucumentHourlySalary(ExpensePeriod, model.Authentication);

                    var sgkDocument = documentManager.ComputeExpenseDucumentHourlySGK(document.ID, ExpensePeriod, model.Authentication);

                    var FoodDocument = documentManager.ComputeExpenseDucumentHourlyFoodCard(document.ID, ExpensePeriod, model.Authentication);

                    var PrimDocument = documentManager.ComputeExpenseDucumentHourlyPremium(document.ID, ExpensePeriod, model.Authentication);

                }
                if (Office == "1")
                {
                    var document = documentManager.ComputeExpenseDucumentMonthlySalary(ExpensePeriod, model.Authentication);

                    foreach (var item in Db.ExpenseDocument.Where(x => x.ExpensePeriodCode == ExpensePeriod && x.ExpenseGroupID == 2 && x.ExpenseItemID == 5).ToList())
                    {
                        var salaryDocument = documentManager.ComputeExpenseDucumentMontlySalaryChart(item.ID, ExpensePeriod, model.Authentication);
                        var sgkDocument = documentManager.ComputeExpenseDucumentMontlySGK(item.ID, item.ExpenseCenterID, ExpensePeriod, model.Authentication);
                        var foodDocument = documentManager.ComputeExpenseDucumentMontlyFoodCard(item.ID, item.ExpenseCenterID, ExpensePeriod, model.Authentication);
                        var premDocument = documentManager.ComputeExpenseDucumentMontlyPremium(item.ID, item.ExpenseCenterID, ExpensePeriod, model.Authentication);
                    }

                    var documentg = documentManager.ComputeExpenseDucumentKidem(ExpensePeriod, model.Authentication);
                    var gDocument = documentManager.ComputeExpenseDucumentMontlyKidemChart(documentg.ID, ExpensePeriod, model.Authentication);
                }

                if (Rent == "1")
                {
                    var documentL = documentManager.ComputeExpenseDucumentLocationRent(ExpensePeriod, model.Authentication);

                    var documentO = documentManager.ComputeExpenseDucumentOfficeRent(ExpensePeriod, model.Authentication);
                }

                if (Vat == "1")
                {
                    var document = documentManager.ComputeExpenseDucumentVat(ExpensePeriod, model.Authentication);
                }

                if (Expense == "1")
                {
                    var documentL = documentManager.ComputeExpenseDucumentLocationExpense(ExpensePeriod, model.Authentication);

                    var documentB = documentManager.ComputeExpenseDucumentBankExpense(ExpensePeriod, model.Authentication);
                }

                if (Manuel == "1")
                {
                    var document = documentManager.ComputeExpenseDucumentManuel(ExpensePeriod, model.Authentication);
                }

                model.Result.IsSuccess = true;
                model.Result.Message = "Hesaplama Tamamlandı";

                if (Reset == "1")
                {
                    var documentr = documentManager.ResetExpenseDucument(ExpensePeriod, model.Authentication);

                    model.Result.IsSuccess = documentr.IsSuccess;
                    model.Result.Message = documentr.Message;
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Dönem Seçilmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("NewDocument", "Expense");
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

            return RedirectToAction("Index", "Expense", new { id });
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ArrangeByGroup(ArrangeFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            if (form.ECGID == null || form.EUID == null)
            {
                return RedirectToAction("Index");
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
                return RedirectToAction("Index");
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

            return RedirectToAction("Chart", "Expense", new { id = model.ExpenseDocument.UID });

        }

        [AllowAnonymous]
        public FileResult GetExpenseTemplate()
        {
            ExpenseControlModel model = new ExpenseControlModel();

            string targetpath = Server.MapPath("~/Document/Expense/");
            string FileName = $"ExpenseTemplate.xlsx";

            var isCreated = CreateExcelExpense(FileName);

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

        public bool CreateExcelExpense(string FileName)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            bool isSuccess = false;

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("ExpenseTemplate");

                    worksheet.Cell("A1").Value = "MasrafMerkezi";
                    worksheet.Cell("B1").Value = "MasrafGrubu";
                    worksheet.Cell("C1").Value = "MasrafKalemi";
                    worksheet.Cell("D1").Value = "DagitimTutari";
                    worksheet.Cell("E1").Value = "KDVOrani";
                    worksheet.Cell("F1").Value = "DagitimGrubu";

                    worksheet.Cell("A2").Value = 131;
                    worksheet.Cell("B2").Value = 2;
                    worksheet.Cell("C2").Value = 31;
                    worksheet.Cell("D2").Value = 10000;
                    worksheet.Cell("E2").Value = 18;
                    worksheet.Cell("F2").Value = "T";

                    //MasrafMerkezi
                    var worksheet1 = workbook.Worksheets.Add("MasrafMerkezi");

                    worksheet1.Cell("A1").Value = "ID";
                    worksheet1.Cell("B1").Value = "Masraf Merkezi";
                    worksheet1.Cell("C1").Value = "Türü";

                    int rownum = 2;
                    var denyLocationIds = new List<int>() { 179, 175, 212 }.ToList();
                    var expenseCenters = Db.VLocation.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true && !denyLocationIds.Contains(x.LocationID)).Select(x => new { ID = x.LocationID, Name = x.LocationFullName, TypeName = x.LocationTypeName }).ToList();
                    foreach (var item in expenseCenters.OrderBy(x => x.Name))
                    {

                        worksheet1.Cell("A" + rownum).Value = item.ID;
                        worksheet1.Cell("B" + rownum).Value = item.Name;
                        worksheet1.Cell("C" + rownum).Value = item.TypeName;

                        rownum++;
                    }

                    //MasrafGrubu
                    var worksheet2 = workbook.Worksheets.Add("MasrafGrubu");

                    worksheet2.Cell("A1").Value = "ID";
                    worksheet2.Cell("B1").Value = "Masraf Grubu";

                    rownum = 2;

                    var expenseGroups = Db.ExpenseGroup.ToList();
                    foreach (var item in expenseGroups.OrderBy(x => x.SortBy))
                    {

                        worksheet2.Cell("A" + rownum).Value = item.ID;
                        worksheet2.Cell("B" + rownum).Value = item.ExpenseGroupName;

                        rownum++;
                    }

                    //MasrafKalemi
                    var worksheet3 = workbook.Worksheets.Add("MasrafKalemi");

                    worksheet3.Cell("A1").Value = "ID";
                    worksheet3.Cell("B1").Value = "Masraf Kalemleri";

                    rownum = 2;

                    var expenseItems = Db.ExpenseItem.ToList();
                    foreach (var item in expenseItems.OrderBy(x => x.SortBy))
                    {

                        worksheet3.Cell("A" + rownum).Value = item.ID;
                        worksheet3.Cell("B" + rownum).Value = item.ExpenseItemName;

                        rownum++;
                    }

                    //DagitimGrubu
                    var worksheet4 = workbook.Worksheets.Add("DagitimGrubu");

                    worksheet4.Cell("A1").Value = "ID";
                    worksheet4.Cell("B1").Value = "Kodu";
                    worksheet4.Cell("C1").Value = "Dağıtım Grubu";

                    rownum = 2;

                    var distributeGroups = Db.ExpenseChartGroup.Where(x => x.IsActive == true).ToList();
                    foreach (var item in distributeGroups.OrderBy(x => x.SortBy))
                    {
                        worksheet4.Cell("A" + rownum).Value = item.ID;
                        worksheet4.Cell("B" + rownum).Value = item.GroupCode;
                        worksheet4.Cell("C" + rownum).Value = item.GroupName;

                        rownum++;
                    }


                    string targetpath = Server.MapPath("~/Document/Expense/");
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


        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddExpenseImport(FormExpenseDocumentImport form)
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
                return RedirectToAction("NewDocument");
            }

            var expensePeriod = Db.ExpensePeriod.FirstOrDefault(x => x.PeriodCode == form.ExpensePeriod);
            var datalistforlog = new List<ExcelExpenseDocument>();
            DateTime documentDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone ?? 3).Date;

            if (expensePeriod != null)
            {
                List<ExpenseDocument> expenseDocuments = new List<ExpenseDocument>();
                List<ExpenseDocumentRows> expenseRowList = new List<ExpenseDocumentRows>();
                List<ExpenseDocumentChart> expenseChartList = new List<ExpenseDocumentChart>();


                if (form.ExpenseFile != null && form.ExpenseFile.ContentLength > 0)
                {

                    if (form.ExpenseFile.ContentType == "application/vnd.ms-excel" || form.ExpenseFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(form.ExpenseFile.FileName);
                        string targetpath = Server.MapPath("~/Document/Expense/");
                        string pathToExcelFile = targetpath + filename;


                        form.ExpenseFile.SaveAs(Path.Combine(targetpath, filename));



                        var connectionString = "";
                        if (filename.EndsWith(".xls"))
                        {
                            connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", pathToExcelFile);
                        }
                        else if (filename.EndsWith(".xlsx"))
                        {
                            connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", pathToExcelFile);
                        }


                        string sheetName = "ExpenseTemplate";
                        var excelFile = new ExcelQueryFactory(pathToExcelFile);
                        var expenseList = from a in excelFile.Worksheet<ExcelExpenseDocument>(sheetName) select a;
                        datalistforlog = expenseList.ToList();

                        foreach (var item in datalistforlog)
                        {
                            var distGrup = Db.ExpenseChartGroup.FirstOrDefault(x => x.GroupCode == item.DagitimGrubu);
                            if (distGrup == null)
                            {
                                distGrup = Db.ExpenseChartGroup.FirstOrDefault(x => x.ID == 5);
                            }

                            var document = new ExpenseDocument();
                            var UID = Guid.NewGuid();

                            document.UID = UID;
                            document.DocumentNumber = OfficeHelper.GetDocumentNumber(model.Authentication.ActionEmployee.OurCompanyID ?? 2, "EXD");
                            document.RecordDate = DateTime.UtcNow.AddHours(3);
                            document.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                            document.RecordIP = OfficeHelper.GetIPAddress();
                            document.DocumentSource = "Excel";
                            document.ExpenseDescription = "";
                            document.DistributionAmount = item.DagitimTutari;
                            document.TotalAmount = item.DagitimTutari;
                            document.ExpenseGroupID = item.MasrafGrubu;
                            document.Currency = model.Authentication.ActionEmployee.OurCompany.Currency;
                            document.DocumentDate = documentDate;
                            document.ExpenseItemID = item.MasrafKalemi;
                            document.ExpenseCenterID = item.MasrafMerkezi;
                            document.ExpensePeriod = expensePeriod.DateBegin;
                            document.IsActive = true;
                            document.StatusID = 0;
                            document.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID;
                            document.TaxRate = item.KDVOrani;
                            document.DistributeGroupID = distGrup.ID;

                            if (form.ExpensePeriod != null)
                            {
                                document.ExpenseYear = document.ExpensePeriod.Value.Year;
                                document.ExpenseMonth = document.ExpensePeriod.Value.Month;
                                document.ExpensePeriodCode = document.ExpenseYear.ToString() + "-" + (document.ExpenseMonth <= 9 ? "0" + document.ExpenseMonth.ToString() : document.ExpenseMonth.ToString());
                            }

                            Db.ExpenseDocument.Add(document);
                            Db.SaveChanges();

                            expenseDocuments.Add(document); //listeye eklenir

                            model.Result.IsSuccess = true;
                            model.Result.Message = "Masraf Dokümanı Eklendi";

                            OfficeHelper.AddApplicationLog("Office", "ExpenseDocument", "Insert", document.ID.ToString(), "Expense", "AddExpenseImport", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, document);

                        }

                        // rowslar dağıtım cetveli ve maliyetler eklenir

                        foreach (var item in expenseDocuments)
                        {

                            if (item.ExpenseGroupID == 2 || item.ExpenseGroupID == 3)
                            {
                                if (item.DistributeGroupID == 1 || item.DistributeGroupID == 2 || item.DistributeGroupID == 3)
                                {
                                    var dresult = Db.AddExpenseDocumentGroupRowsChart(item.ID, item.DistributeGroupID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress()).FirstOrDefault().Value;
                                }
                            }
                            else if (item.ExpenseGroupID == 1)
                            {
                                if (item.DistributeGroupID == 4)
                                {
                                    var dresult = Db.AddExpenseDocumentGroupRowsChartLoc(item.ID, item.DistributeGroupID, item.ExpenseCenterID, model.Authentication.ActionEmployee.EmployeeID, OfficeHelper.GetIPAddress()).FirstOrDefault().Value;
                                }
                            }
                        }

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
                model.Result.Message = $"Masraf Dokümanı bilgisi bulunamadı";
            }

            model.Result.IsSuccess = false;
            model.Result.Message = $"Masraf Dokümanı ekleme tamamlandı";

            TempData["result"] = model.Result;

            return RedirectToAction("Index");

        }

        //AddDistGroup
        [AllowAnonymous]
        public ActionResult DistGroup(Guid? id, string period)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.ExpenseChartGroups = Db.ExpenseChartGroup.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (id != null)
            {
                model.ExpenseChartGroup = Db.ExpenseChartGroup.FirstOrDefault(x => x.UID == id);
            }
            else
            {
                model.ExpenseChartGroup = Db.ExpenseChartGroup.FirstOrDefault(x => x.ID > 0);
            }

            model.ExpensePeriods = Db.ExpensePeriod.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            if (string.IsNullOrEmpty(period))
            {
                period = "2022-01";
            }

            model.ExpensePeriod = model.ExpensePeriods.FirstOrDefault(x => x.PeriodCode == period);
            model.ExpenseChartGroupItems = Db.ExpenseChartGroupItems.Where(x => x.PeriodCode == model.ExpensePeriod.PeriodCode && x.ChartGroupID == model.ExpenseChartGroup.ID).ToList();

            List<int> noTypeId = new List<int>() { 5, 6 };
            List<int> noTestId = new List<int>() { 175, 179, 212 };
            List<int> GroupId = model.ExpenseChartGroupItems.Select(x => x.LocationID).ToList();

            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && !noTypeId.Contains(x.LocationTypeID.Value) && !noTestId.Contains(x.LocationID) && GroupId.Contains(x.LocationID)).ToList();
            model.OutLocations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && !noTypeId.Contains(x.LocationTypeID.Value) && !noTestId.Contains(x.LocationID) && !GroupId.Contains(x.LocationID)).ToList();

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult RemoveDistItems(int[] GroupItems, short GroupID, Guid? GroupUID, string PeriodCode)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();

            var ItemIds = GroupItems.ToList();

            var groupItems = Db.ExpenseChartGroupItems.Where(x => x.ChartGroupID == GroupID && ItemIds.Contains(x.ID)).ToList();

            if (groupItems.Count > 0)
            {
                Db.ExpenseChartGroupItems.RemoveRange(groupItems);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = "Seçili Lokasyonlar Kaldırıldı.";
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Seçili Lokasyon Bulunamadı.";
            }


            TempData["Result"] = model.Result;

            return RedirectToAction("DistGroup", "Expense", new { id = GroupUID, period = PeriodCode });
        }



        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddDistItems(int[] LocationIds, short GroupID, Guid? GroupUID, string PeriodCode)
        {
            ExpenseControlModel model = new ExpenseControlModel();
            model.Result = new Result();
            List<ExpenseChartGroupItems> expenseChartGroupItems = new List<ExpenseChartGroupItems>();

            var ItemIds = LocationIds.ToList();

            foreach (var item in ItemIds)
            {
                expenseChartGroupItems.Add(new ExpenseChartGroupItems()
                {
                    ChartGroupID = GroupID,
                    LocationID = item,
                    PeriodCode = PeriodCode
                });
            }

            if (expenseChartGroupItems.Count > 0)
            {
                Db.ExpenseChartGroupItems.AddRange(expenseChartGroupItems);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = "Seçili Lokasyonlar Eklendi.";
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Seçili Lokasyon Bulunamadı.";
            }


            TempData["Result"] = model.Result;

            return RedirectToAction("DistGroup", "Expense", new { id = GroupUID, period = PeriodCode });
        }











        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddDistGroup(DistGroupFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (form != null && !string.IsNullOrEmpty(form.GroupName))
            {
                var dgroup = Db.ExpenseChartGroup.FirstOrDefault(x => x.GroupCode == form.GroupCode || x.GroupName.Trim() == form.GroupName.Trim());

                if (dgroup == null)
                {
                    dgroup = new ExpenseChartGroup();
                    //form.GroupUID = Guid.NewGuid();

                    dgroup.UID = form.GroupUID;
                    dgroup.IsActive = true;
                    dgroup.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID.Value;
                    dgroup.GroupCode = form.GroupCode;
                    dgroup.GroupName = form.GroupName;
                    dgroup.SortBy = form.SortBy;

                    Db.ExpenseChartGroup.Add(dgroup);
                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Dagılım Grubu Eklendi";

                    OfficeHelper.AddApplicationLog("Office", "ExpenseChartGroup", "Insert", dgroup.ID.ToString(), "Expense", "AddDistGroup", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, dgroup);
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Benzer Masraf Dagılım Grubu Bulundu";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("DistGroup", "Expense", new { id = form.GroupUID });
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditDistGroup(DistGroupFormModel form)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            if (form != null && !string.IsNullOrEmpty(form.GroupName))
            {
                var dgroup = Db.ExpenseChartGroup.FirstOrDefault(x => x.ID == form.GroupID && x.UID == form.GroupUID);

                if (dgroup != null)
                {
                    ExpenseChartGroup self = new ExpenseChartGroup()
                    {
                        GroupCode = dgroup.GroupCode,
                        GroupName = dgroup.GroupName,
                        ID = dgroup.ID,
                        UID = dgroup.UID,
                        IsActive = dgroup.IsActive,
                        OurCompanyID = dgroup.OurCompanyID,
                        SortBy = dgroup.SortBy
                    };

                    dgroup.GroupCode = form.GroupCode;
                    dgroup.GroupName = form.GroupName;
                    dgroup.SortBy = form.SortBy;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Masraf Dagılım Grubu Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<ExpenseChartGroup>(self, dgroup, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "ExpenseDocument", "Update", dgroup.ID.ToString(), "Expense", "Detail", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Benzer Masraf Dagılım Grubu Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("DistGroup", "Expense", new { id = form.GroupUID });
        }


    }
}