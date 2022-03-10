using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ExpenseController : BaseController
    {

        [AllowAnonymous]
        public ActionResult Index(int? ECID, int? EIID, int? ESID, string EPCD)
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

            IQueryable<VExpenseDocument> expenseDocuments;

            if (model.Filters.FromSearch == true || ECID != null || EIID != null || ESID != null || !string.IsNullOrEmpty(EPCD))
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
        public ActionResult ExpenseFilter(int? ECID, int? EIID, int? ESID, string EPCD, DateTime? DTBG, DateTime DTEN)
        {
            ExpenseFilterModel model = new ExpenseFilterModel();

            model.ExpenseCenterID = ECID ?? null;
            model.ExpenseItemID = EIID ?? null;
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
                        TotalAmount = document.TotalAmount
                    };

                    var totalAmount = Convert.ToDouble(form.TotalAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                    var distAmount = Convert.ToDouble(form.DistributionAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);

                    document.ExpenseDescription = form.ExpenseDescription;

                    if (document.StatusID == 0)
                    {
                        document.DistributionAmount = distAmount;
                        document.TotalAmount = totalAmount;
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

            if (model.ExpenseDocument == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Index");
            }

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

            var charts = Db.ExpenseDocumentChart.Where(x => x.ExpenseDocumentID == expenseDocument.ID).ToList();

            foreach (var item in charts)
            {
                ExpenseActions exa = new ExpenseActions();

                exa.Amount = item.DistributedAmount;
                exa.Currency = item.Currency;
                exa.DocumentID = item.ExpenseDocumentID;
                exa.DocumentNumber = expenseDocument.DocumentNumber;
                exa.ExpenseCenterID = (short)expenseDocument.ExpenseCenterID;
                exa.ExpenseDescription = expenseDocument.ExpenseDescription;
                exa.ExpenseGroupID = expenseDocument.ExpenseGroupID;
                exa.ExpenseItemID = item.ExpenseItemID;
                exa.ExpenseMonth = expenseDocument.ExpenseMonth;
                exa.ExpensePeriod = expenseDocument.ExpensePeriod;
                exa.ExpensePeriodCode = expenseDocument.ExpensePeriodCode;
                exa.ExpenseYear = expenseDocument.ExpenseYear;
                exa.LocationID = (short)item.ExpenseCenterID;
                exa.OurCompanyID = expenseDocument.OurCompanyID;
                exa.Rate = item.DistributedRate;
                exa.RecordDate = DateTime.UtcNow.AddHours(3);
                exa.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                exa.RecordIP = OfficeHelper.GetIPAddress();
                exa.ReferenceUID = expenseDocument.UID;
                exa.Total = item.DistributionAmount;

                expenseActionsList.Add(exa);
            }

            Db.ExpenseActions.AddRange(expenseActionsList);
            Db.SaveChanges();

            expenseDocument.StatusID = 1;
            Db.SaveChanges();

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
        public ActionResult AddExpenseAuto(string ExpensePeriod, string Location, string Office, string Setcard, string Rent, string Vat, string Expense)
        {
            ExpenseControlModel model = new ExpenseControlModel();

            model.Result = new Result();

            DocumentManager documentManager = new DocumentManager();

 
            if (!string.IsNullOrEmpty(ExpensePeriod))
            {
                if (Location == "1")
                {
                    var document = documentManager.ComputeExpenseDucumentHourlySalary(ExpensePeriod, model.Authentication);
                }
                if (Office == "1")
                {

                }
                if (Setcard == "1")
                {

                }
                if (Rent == "1")
                {

                }
                if (Vat == "1")
                {

                }
                if (Expense == "1")
                {

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






    }
}