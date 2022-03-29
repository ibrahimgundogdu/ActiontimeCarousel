using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class PartnerController : BaseController
    {

        [AllowAnonymous]
        public ActionResult Index()
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Payments(int? PartnerID, int? LocationID, string ExpensePeriodCode)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as PartnerFilterModel;
            }
            else
            {
                PartnerFilterModel filterModel = new PartnerFilterModel();

                filterModel.PartnerID = PartnerID ?? null;
                filterModel.LocationID = LocationID ?? null;
                filterModel.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
                filterModel.DateBegin = new DateTime(DateTime.Now.Year, 1, 1);
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            List<int> locationIds = Db.Partnership.Where(x => x.PartnerID > 0).Select(x => x.LocationID.Value).Distinct().ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            IQueryable<VDocumentPartnerPayment> paymentDocuments;

            if (model.Filters.PartnerID != null || model.Filters.LocationID != null || model.Filters.DateBegin != null || model.Filters.DateEnd != null || !string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
            {
                paymentDocuments = Db.VDocumentPartnerPayment.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.PartnerID != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.PartnerID == model.Filters.PartnerID);
                }

                if (model.Filters.LocationID != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.LocationID == model.Filters.LocationID);
                }

                if (!string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
                {
                    paymentDocuments = paymentDocuments.Where(x => x.PeriodCode == model.Filters.ExpensePeriodCode);
                }

                if (model.Filters.DateBegin != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.DocumentDate >= model.Filters.DateBegin);
                }

                if (model.Filters.DateEnd != null)
                {
                    paymentDocuments = paymentDocuments.Where(x => x.DocumentDate <= model.Filters.DateEnd);
                }

                model.DocumentPartnerPayments = paymentDocuments.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult PaymentFilter(int? PartnerID, int? LocationID, string ExpensePeriodCode, DateTime? DateBegin, DateTime? DateEnd)
        {
            PartnerFilterModel model = new PartnerFilterModel();

            model.PartnerID = PartnerID ?? null;
            model.LocationID = LocationID ?? null;
            model.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
            model.DateBegin = DateBegin != null ? DateBegin : new DateTime(DateTime.Now.Year, 1, 1);
            model.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.Date;
                model.DateBegin = new DateTime(begin.Year, 1, 1);
            }

            if (DateEnd == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Payments", "Partner");
        }

        [AllowAnonymous]
        public ActionResult NewPaymentDocument()
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddPayment(PartnerPaymentFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var totalAmount = Convert.ToDouble(form.PaymentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var partnership = Db.Partnership.FirstOrDefault(x => x.ID == form.PartnershipID);
                var exchange = OfficeHelper.GetExchange(form.DocumentDate);
                var currency = model.Authentication.ActionEmployee.OurCompany.Currency;

                var document = new DocumentPartnerPayment();

                form.UID = Guid.NewGuid();

                document.UID = form.UID;
                document.DocumentNumber = OfficeHelper.GetDocumentNumber(model.Authentication.ActionEmployee.OurCompanyID ?? 2, "PP");
                document.RecordDate = DateTime.UtcNow.AddHours(3);
                document.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                document.RecordIP = OfficeHelper.GetIPAddress();
                document.ReferenceNumber = form.DocumentSource;
                document.Description = form.PaymentDescription;
                document.Amount = totalAmount;
                document.Currency = currency;
                document.DocumentDate = form.DocumentDate;
                document.PeriodCode = form.ExpensePeriodCode;
                document.IsActive = true;
                document.OurCompanyID = model.Authentication.ActionEmployee.OurCompanyID;
                document.LocationID = partnership?.LocationID;
                document.PartnerID = partnership?.PartnerID;
                document.PayMethodID = form.PayMethodID;
                document.PartnerActionTypeID = 2;
                document.ActionTypeName = "Ödeme";
                document.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                document.SystemAmount = document.Amount * document.ExchangeRate;
                document.SystemCurrency = currency;

                Db.DocumentPartnerPayment.Add(document);
                Db.SaveChanges();

                model.Result.IsSuccess = true;
                model.Result.Message = "Partner Ödeme Dokümanı Eklendi";

                OfficeHelper.AddApplicationLog("Office", "PaymentDocument", "Insert", document.ID.ToString(), "Partner", "AddPayment", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, document);

                // partner cari hareketi eklenir
                try
                {
                    Db.AddPartnerPaymentToAction(document.ID, model.Authentication.ActionEmployee.EmployeeID);
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("PaymentDetail", "Partner", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult PaymentDetail(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }


            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();
            model.DocumentPartnerPayment = Db.VDocumentPartnerPayment.FirstOrDefault(x => x.UID == id);

            if (model.DocumentPartnerPayment == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Payments");
            }

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdatePayment(PartnerPaymentFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {
                var totalAmount = Convert.ToDouble(form.PaymentAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var partnership = Db.Partnership.FirstOrDefault(x => x.ID == form.PartnershipID);
                var exchange = OfficeHelper.GetExchange(form.DocumentDate);
                var currency = model.Authentication.ActionEmployee.OurCompany.Currency;

                var document = Db.DocumentPartnerPayment.FirstOrDefault(x => x.UID == form.UID && x.ID == form.DocumentPaymentID);

                if (document != null)
                {

                    DocumentPartnerPayment self = new DocumentPartnerPayment()
                    {
                        RecordDate = document.RecordDate,
                        RecordEmployeeID = document.RecordEmployeeID,
                        RecordIP = document.RecordIP,
                        IsActive = document.IsActive,
                        OurCompanyID = document.OurCompanyID,
                        Currency = document.Currency,
                        UID = document.UID,
                        DocumentNumber = document.DocumentNumber,
                        DocumentDate = document.DocumentDate,
                        ID = document.ID,
                        UpdateEmployee = document.UpdateEmployee,
                        ActionTypeName = document.ActionTypeName,
                        Amount = document.Amount,
                        Description = document.Description,
                        ExchangeRate = document.ExchangeRate,
                        FromBankAccountID = document.FromBankAccountID,
                        LocationID = document.LocationID,
                        PartnerActionTypeID = document.PartnerActionTypeID,
                        PartnerID = document.PartnerID,
                        PayMethodID = document.PayMethodID,
                        PeriodCode = document.PeriodCode,
                        ReferenceNumber = document.ReferenceNumber,
                        SystemAmount = document.SystemAmount,
                        SystemCurrency = document.SystemCurrency,
                        UpdateDate = document.UpdateDate,
                        UpdateIP = document.UpdateIP
                    };


                    document.UpdateDate = DateTime.UtcNow.AddHours(3);
                    document.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                    document.UpdateIP = OfficeHelper.GetIPAddress();
                    document.ReferenceNumber = form.DocumentSource;
                    document.Description = form.PaymentDescription;
                    document.Amount = totalAmount;
                    document.DocumentDate = form.DocumentDate;
                    document.PeriodCode = form.ExpensePeriodCode;
                    document.IsActive = form.IsActive == "1" ? true : false;
                    document.LocationID = partnership?.LocationID;
                    document.PartnerID = partnership?.PartnerID;
                    document.PayMethodID = form.PayMethodID;
                    document.ExchangeRate = currency == "USD" ? exchange.USDA : currency == "EUR" ? exchange.EURA : 1;
                    document.SystemAmount = document.Amount * document.ExchangeRate;
                    document.SystemCurrency = currency;

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Partner Ödeme Dokümanı Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPartnerPayment>(self, document, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "PaymentDocument", "Update", document.ID.ToString(), "Partner", "PaymentDetail", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    // partner cari hareketi eklenir
                    try
                    {
                        Db.AddPartnerPaymentToAction(document.ID, model.Authentication.ActionEmployee.EmployeeID);
                    }
                    catch (Exception ex)
                    {
                    }

                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Doküman Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("PaymentDetail", "Partner", new { id = form.UID });
        }


        //Earns
        [AllowAnonymous]
        public ActionResult Earns(int? PartnerID, int? LocationID, string ExpensePeriodCode)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as PartnerFilterModel;
            }
            else
            {
                PartnerFilterModel filterModel = new PartnerFilterModel();

                filterModel.PartnerID = PartnerID ?? null;
                filterModel.LocationID = LocationID ?? null;
                filterModel.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
                filterModel.DateBegin = new DateTime(DateTime.Now.Year, 1, 1);
                filterModel.DateEnd = DateTime.Now.Date;
                model.Filters = filterModel;
            }

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            List<int> locationIds = Db.Partnership.Where(x => x.PartnerID > 0).Select(x => x.LocationID.Value).Distinct().ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            IQueryable<VDocumentPartnerEarn> earnDocuments;

            if (model.Filters.PartnerID != null || model.Filters.LocationID != null || model.Filters.DateBegin != null || model.Filters.DateEnd != null || !string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
            {
                earnDocuments = Db.VDocumentPartnerEarn.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.PartnerID != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.PartnerID == model.Filters.PartnerID);
                }

                if (model.Filters.LocationID != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.LocationID == model.Filters.LocationID);
                }

                if (!string.IsNullOrEmpty(model.Filters.ExpensePeriodCode))
                {
                    earnDocuments = earnDocuments.Where(x => x.PeriodCode == model.Filters.ExpensePeriodCode);
                }

                if (model.Filters.DateBegin != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.DocumentDate >= model.Filters.DateBegin);
                }

                if (model.Filters.DateEnd != null)
                {
                    earnDocuments = earnDocuments.Where(x => x.DocumentDate <= model.Filters.DateEnd);
                }

                model.DocumentPartnerEarns = earnDocuments.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EarnsFilter(int? PartnerID, int? LocationID, string ExpensePeriodCode, DateTime? DateBegin, DateTime? DateEnd)
        {
            PartnerFilterModel model = new PartnerFilterModel();

            model.PartnerID = PartnerID ?? null;
            model.LocationID = LocationID ?? null;
            model.ExpensePeriodCode = !string.IsNullOrEmpty(ExpensePeriodCode) ? ExpensePeriodCode : string.Empty;
            model.DateBegin = DateBegin != null ? DateBegin : new DateTime(DateTime.Now.Year, 1, 1);
            model.DateEnd = DateEnd != null ? DateEnd : DateTime.Now.Date;

            if (DateBegin == null)
            {
                DateTime begin = DateTime.Now.Date;
                model.DateBegin = new DateTime(begin.Year, 1, 1);
            }

            if (DateEnd == null)
            {
                model.DateEnd = DateTime.Now.Date;
            }

            TempData["filter"] = model;

            return RedirectToAction("Earns", "Partner");
        }

        [AllowAnonymous]
        public ActionResult EarnDetail(Guid? id)
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (id == null)
            {
                return RedirectToAction("Index");
            }


            model.Partnerships = Db.VPartnership.Where(x => x.PartnerID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();
            model.PayMethods = Db.PayMethod.Where(x => x.IsActive == true).ToList();
            model.DocumentPartnerEarn = Db.VDocumentPartnerEarn.FirstOrDefault(x => x.UID == id);

            if (model.DocumentPartnerEarn == null)
            {
                TempData["Result"] = model.Result;
                return RedirectToAction("Earns");
            }

            model.DocumentPartnerEarnRows = Db.VDocumentPartnerEarnRow.Where(x => x.DocumentID == model.DocumentPartnerEarn.ID).ToList();

            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult UpdateEarn(PartnerEarnFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null)
            {


                var document = Db.DocumentPartnerEarn.FirstOrDefault(x => x.UID == form.UID && x.ID == form.DocumentEarnID);

                if (document != null)
                {

                    DocumentPartnerEarn self = new DocumentPartnerEarn()
                    {
                        RecordDate = document.RecordDate,
                        RecordEmployeeID = document.RecordEmployeeID,
                        RecordIP = document.RecordIP,
                        IsActive = document.IsActive,
                        OurCompanyID = document.OurCompanyID,
                        Currency = document.Currency,
                        UID = document.UID,
                        DocumentNumber = document.DocumentNumber,
                        DocumentDate = document.DocumentDate,
                        ID = document.ID,
                        UpdateEmployee = document.UpdateEmployee,
                        ActionTypeName = document.ActionTypeName,
                        Amount = document.Amount,
                        Description = document.Description,
                        ExchangeRate = document.ExchangeRate,
                        FromBankAccountID = document.FromBankAccountID,
                        LocationID = document.LocationID,
                        PartnerActionTypeID = document.PartnerActionTypeID,
                        PartnerID = document.PartnerID,
                        PeriodCode = document.PeriodCode,
                        ReferenceNumber = document.ReferenceNumber,
                        SystemAmount = document.SystemAmount,
                        SystemCurrency = document.SystemCurrency,
                        UpdateDate = document.UpdateDate,
                        UpdateIP = document.UpdateIP

                    };

                    try
                    {
                        Db.AddPartnerShipEarnDocument(document.PartnerID, document.LocationID, document.ID, document.PeriodCode, model.Authentication.ActionEmployee.EmployeeID);
                    }
                    catch (Exception ex)
                    {
                    }

                    document.UpdateDate = DateTime.UtcNow.AddHours(3);
                    document.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                    document.UpdateIP = OfficeHelper.GetIPAddress();

                    Db.SaveChanges();

                    model.Result.IsSuccess = true;
                    model.Result.Message = "Partner Hakediş Dokümanı Güncellendi";

                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentPartnerEarn>(self, document, OfficeHelper.getIgnorelist());
                    OfficeHelper.AddApplicationLog("Office", "EarnDocument", "Update", document.ID.ToString(), "Partner", "EarnDetail", isequal, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    // partner cari hareketi eklenir
                    try
                    {
                        Db.AddPartnerPaymentToAction(document.ID, model.Authentication.ActionEmployee.EmployeeID);
                    }
                    catch (Exception ex)
                    {
                    }

                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Doküman Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("EarnDetail", "Partner", new { id = form.UID });
        }

        [AllowAnonymous]
        public ActionResult NewEarnDocument()
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            model.Partners = Db.Partner.Where(x => x.ID > 0).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddEarn(PartnerEarnFormModel form)
        {
            PartnerControlModel model = new PartnerControlModel();

            model.Result = new Result();

            if (form != null && form.PartnerID != null && !string.IsNullOrEmpty(form.ExpensePeriodCode))
            {

                var partner = Db.Partner.FirstOrDefault(x => x.ID == form.PartnerID);
                var period = Db.ExpensePeriod.FirstOrDefault(x => x.PeriodCode == form.ExpensePeriodCode);

                if (partner != null && period != null)
                {
                    try
                    {
                        string ipAddress = OfficeHelper.GetIPAddress();

                        var sresult = Db.AddPartnerEarnDocument(partner.ID, period.PeriodCode, model.Authentication.ActionEmployee.EmployeeID, ipAddress).FirstOrDefault();

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Partner Hakediş Dokümanı Eklendi / Güncelleştirildi";
                        OfficeHelper.AddApplicationLog("Office", "EarnDocument", "Insert", null, "Partner", "AddEarn", null, true, $"{model.Result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {
                        model.Result.IsSuccess = false;
                        model.Result.Message = "Partner Hakediş Dokümanı İşlenemedi!";
                    }
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Form Verileri Gelmedi";
            }

            TempData["Result"] = model.Result;

            return RedirectToAction("Earns", "Partner");
        }

        //Actions
        [AllowAnonymous]
        public ActionResult Actions(int? PartnerID, int? LocationID, string PeriodCodeEnd, string PeriodCodeBegin = "2022-01")
        {
            PartnerControlModel model = new PartnerControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result ?? null;
            }

            if (TempData["filter"] != null)
            {
                model.Filters = TempData["filter"] as PartnerFilterModel;
            }
            else
            {
                PartnerFilterModel filterModel = new PartnerFilterModel();

                filterModel.PartnerID = PartnerID ?? null;
                filterModel.LocationID = LocationID ?? null;
                filterModel.PeriodCodeBegin = !string.IsNullOrEmpty(PeriodCodeBegin) ? PeriodCodeBegin : "2022-01";
                filterModel.PeriodCodeEnd = !string.IsNullOrEmpty(PeriodCodeEnd) ? PeriodCodeEnd : DateTime.Now.ToString("yyyy-MM");

                model.Filters = filterModel;
            }

            model.Partners = Db.Partner.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ID > 0).ToList();
            List<int> locationIds = Db.Partnership.Where(x => x.PartnerID > 0).Select(x => x.LocationID.Value).Distinct().ToList();
            model.Locations = Db.Location.Where(x => locationIds.Contains(x.LocationID)).ToList();
            model.ExpensePeriods = Db.ExpensePeriod.OrderBy(x => x.DateBegin).ToList();

            var beginperiod = model.ExpensePeriods.FirstOrDefault(x => x.PeriodCode == model.Filters.PeriodCodeBegin);
            var endperiod = model.ExpensePeriods.FirstOrDefault(x => x.PeriodCode == model.Filters.PeriodCodeEnd);

            IQueryable<VPartnerActions> actions;

            if (model.Filters.PartnerID != null || model.Filters.LocationID != null || !string.IsNullOrEmpty(model.Filters.PeriodCodeEnd) || !string.IsNullOrEmpty(model.Filters.PeriodCodeBegin))
            {
                actions = Db.VPartnerActions.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID);

                if (model.Filters.PartnerID != null)
                {
                    actions = actions.Where(x => x.PartnerID == model.Filters.PartnerID);
                }

                if (model.Filters.LocationID != null)
                {
                    actions = actions.Where(x => x.LocationID == model.Filters.LocationID);
                }

                if (!string.IsNullOrEmpty(model.Filters.PeriodCodeBegin))
                {
                    //actions = actions.Where(x => x.ExpenseYear >= beginperiod.DateYear && x.mo);
                }

               

                model.PartnerActions = actions.ToList();
            }

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ActionFilter(int? PartnerID, int? LocationID, string PeriodCodeEnd, string PeriodCodeBegin = "2022-01")
        {
            PartnerFilterModel model = new PartnerFilterModel();

            model.PartnerID = PartnerID;
            model.LocationID = LocationID ?? null;
            model.PeriodCodeBegin = !string.IsNullOrEmpty(PeriodCodeBegin) ? PeriodCodeBegin : string.Empty;
            model.PeriodCodeEnd = !string.IsNullOrEmpty(PeriodCodeEnd) ? PeriodCodeEnd : string.Empty;


            if (string.IsNullOrEmpty(model.PeriodCodeBegin))
            {
                model.PeriodCodeBegin = "2022-01";
            }

            if (string.IsNullOrEmpty(model.PeriodCodeEnd))
            {
                model.PeriodCodeEnd = DateTime.Now.ToString("yyyy-MM");
            }

            TempData["filter"] = model;

            return RedirectToAction("Actions", "Partner");
        }

    }
}