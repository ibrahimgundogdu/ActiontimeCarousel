using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class CashRecorderController : BaseController
    {
        // GET: CashRecorder
        [AllowAnonymous]
        public ActionResult Index(int? locationId)
        {
            CashRecorderControlModel model = new CashRecorderControlModel();

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

            model.CashRecorder = Db.VDocumentCashRecorderSlip.Where(x => x.Date >= model.Filters.DateBegin && x.Date <= model.Filters.DateEnd).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();
            if (model.Filters.LocationID > 0)
            {
                model.CashRecorder = model.CashRecorder.Where(x => x.LocationID == model.Filters.LocationID).OrderByDescending(x => x.Date).ThenByDescending(x => x.RecordDate).ToList();

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

            return RedirectToAction("Index", "CashRecorder");
        }

        [AllowAnonymous]
        public ActionResult Detail(Guid? id)
        {
            CashRecorderControlModel model = new CashRecorderControlModel();

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

            model.Detail = Db.VDocumentCashRecorderSlip.FirstOrDefault(x => x.UID == id);
            model.History = Db.ApplicationLog.Where(x => x.Controller == "CashRecorder" && x.Action == "Index" && x.Environment == "Office" && x.ProcessID == model.Detail.ID.ToString()).ToList();
            

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashRecorder(NewCashRecorder cashRecord, HttpPostedFileBase documentFile)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashRecorderControlModel model = new CashRecorderControlModel();

            if (cashRecord != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashRecord.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashRecord.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var netamount = Convert.ToDouble(cashRecord.NetAmount.Replace(".", ","));
                var totalamount = Convert.ToDouble(cashRecord.TotalAmount.Replace(".", ","));
                var currency = cashRecord.Currency;
                var docDate = DateTime.Now.Date;
                var slipDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashRecord.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashRecord.DocumentDate).Date;
                }
                if (DateTime.TryParse(cashRecord.SlipDate, out slipDate))
                {
                    slipDate = Convert.ToDateTime(cashRecord.SlipDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashRecord.LocationID, cashRecord.Currency);
                
                try
                {
                    var exchange = OfficeHelper.GetExchange(DateTime.UtcNow);

                    DocumentCashRecorderSlip newCashColl = new DocumentCashRecorderSlip();

                    newCashColl.ActionTypeID = actType.ID;
                    newCashColl.ActionTypeName = actType.Name;
                    newCashColl.NetAmount = netamount;
                    newCashColl.TotalAmount = totalamount;
                    newCashColl.Currency = currency;
                    newCashColl.Date = docDate;
                    newCashColl.SlipDate = slipDate;
                    newCashColl.DocumentNumber = OfficeHelper.GetDocumentNumber(location.OurCompanyID, "CR");
                    newCashColl.IsActive = true;
                    newCashColl.LocationID = cashRecord.LocationID;
                    newCashColl.OurCompanyID = location.OurCompanyID;
                    newCashColl.RecordDate = DateTime.UtcNow.AddHours(timezone);
                    newCashColl.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    newCashColl.RecordIP = OfficeHelper.GetIPAddress();
                    newCashColl.SlipNumber = cashRecord.SlipNumber;
                    newCashColl.EnvironmentID = 2;
                    newCashColl.UID = Guid.NewGuid();

                    string FileName = string.Empty;

                    if (documentFile != null)
                    {
                        FileName = Guid.NewGuid().ToString();
                        string ext = System.IO.Path.GetExtension(documentFile.FileName);
                        FileName = FileName + ext;

                        if (documentFile != null && documentFile.ContentLength > 0)
                        {
                            try
                            {
                                documentFile.SaveAs(Path.Combine(Server.MapPath(("../Document/CashRecorder")), FileName));
                                newCashColl.SlipFile = FileName;
                                newCashColl.SlipPath = "Document/CashRecorder";
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }

                    Db.DocumentCashRecorderSlip.Add(newCashColl);
                    Db.SaveChanges();

                    // cari hesap işlemesi
                    //OfficeHelper.AddCashAction(newCashColl.CashID, newCashColl.LocationID, null, newCashColl.ActionTypeID, newCashColl.Date, newCashColl.ActionTypeName, newCashColl.ID, newCashColl.Date, newCashColl.DocumentNumber, newCashColl.Description, -1, 0, newCashColl.Amount, newCashColl.Currency, null, null, newCashColl.RecordEmployeeID, newCashColl.RecordDate);

                    result.IsSuccess = true;
                    result.Message = "Yazarkasa fişi başarı ile eklendi";

                    // log atılır
                    OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Insert", newCashColl.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, newCashColl);


                }
                catch (Exception ex)
                {

                    result.Message = $"Yazarkasa fişi eklenemedi : {ex.Message}";
                    OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Insert", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                }

            }

            TempData["result"] = result;

            return RedirectToAction("Index", "CashRecorder");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashRecorder(NewCashRecorder cashRecord, HttpPostedFileBase documentFile)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashRecorderControlModel model = new CashRecorderControlModel();


            if (cashRecord != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == cashRecord.ActinTypeID);
                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashRecord.LocationID);
                var ourcompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);
                var netamount = Convert.ToDouble(cashRecord.NetAmount.Replace(".", ","));
                var totalamount = Convert.ToDouble(cashRecord.TotalAmount.Replace(".", ","));
                var currency = cashRecord.Currency;
                var docDate = DateTime.Now.Date;
                var slipDate = DateTime.Now.Date;
                int timezone = location.Timezone != null ? location.Timezone.Value : ourcompany.TimeZone.Value;

                if (DateTime.TryParse(cashRecord.DocumentDate, out docDate))
                {
                    docDate = Convert.ToDateTime(cashRecord.DocumentDate).Date;
                }
                if (DateTime.TryParse(cashRecord.SlipDate, out slipDate))
                {
                    slipDate = Convert.ToDateTime(cashRecord.SlipDate).Date;
                }
                var cash = OfficeHelper.GetCash(cashRecord.LocationID, cashRecord.Currency);

                var isDate = DateTime.Now.Date;

                var isCash = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.UID == cashRecord.UID);
                if (isCash != null)
                {
                    try
                    {
                        isDate = Convert.ToDateTime(isCash.Date);

                        DocumentCashRecorderSlip self = new DocumentCashRecorderSlip()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            NetAmount = isCash.NetAmount,
                            TotalAmount = isCash.TotalAmount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            SlipDate = isCash.SlipDate,
                            DocumentNumber = isCash.DocumentNumber,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            SlipNumber = isCash.SlipNumber,
                            SlipFile = isCash.SlipFile,
                            SlipPath = isCash.SlipPath,
                            EnvironmentID = isCash.EnvironmentID
                        };

                        string FileName = string.Empty;

                        if (documentFile != null)
                        {
                            FileName = Guid.NewGuid().ToString();
                            string ext = System.IO.Path.GetExtension(documentFile.FileName);
                            FileName = FileName + ext;

                            if (documentFile != null && documentFile.ContentLength > 0)
                            {
                                try
                                {
                                    documentFile.SaveAs(Path.Combine(Server.MapPath("../Document/CashRecorder"), FileName));
                                    isCash.SlipFile = FileName;
                                    isCash.SlipPath = "Document/CashRecorder";
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        isCash.Date = docDate;
                        isCash.SlipNumber = isCash.SlipNumber;
                        isCash.SlipDate = slipDate;
                        isCash.NetAmount = netamount;
                        isCash.TotalAmount = totalamount;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        //var cashaction = Db.CashActions.FirstOrDefault(x => x.CashID == isKasa && x.LocationID == isCash.LocationID && x.CashActionTypeID == isCash.ActionTypeID && x.ProcessID == isCash.ID && x.ProcessDate == isDate && x.DocumentNumber == isCash.DocumentNumber);

                        //if (cashaction != null)
                        //{
                        //    cashaction.Payment = isCash.Amount;
                        //    cashaction.Currency = cashExpense.Currency;
                        //    cashaction.CashID = cash.ID;
                        //    cashaction.ActionDate = docDate;
                        //    cashaction.ProcessDate = docDate;
                        //    cashaction.UpdateDate = isCash.UpdateDate;
                        //    cashaction.UpdateEmployeeID = isCash.UpdateEmployee;

                        //    Db.SaveChanges();

                        //}

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {totalamount} {currency} tutarındaki yazarkasa fişi başarı ile güncellendi";


                        var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashRecorderSlip>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Update", isCash.ID.ToString(), "CashRecorder", "Index", isequal, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{netamount} {currency} tutarındaki masraf ödeme fişi güncellenemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Update", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Detail", new { id = cashRecord.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashRecorder(int? id)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashRecorderControlModel model = new CashRecorderControlModel();


            if (id != null)
            {

                var isCash = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.ID == id);
                if (isCash != null)
                {
                    try
                    {

                        DocumentCashRecorderSlip self = new DocumentCashRecorderSlip()
                        {
                            ActionTypeID = isCash.ActionTypeID,
                            ActionTypeName = isCash.ActionTypeName,
                            NetAmount = isCash.NetAmount,
                            TotalAmount = isCash.TotalAmount,
                            Currency = isCash.Currency,
                            Date = isCash.Date,
                            SlipDate = isCash.SlipDate,
                            DocumentNumber = isCash.DocumentNumber,
                            ID = isCash.ID,
                            IsActive = isCash.IsActive,
                            LocationID = isCash.LocationID,
                            OurCompanyID = isCash.OurCompanyID,
                            RecordDate = isCash.RecordDate,
                            RecordEmployeeID = isCash.RecordEmployeeID,
                            RecordIP = isCash.RecordIP,
                            UpdateDate = isCash.UpdateDate,
                            UpdateEmployee = isCash.UpdateEmployee,
                            UpdateIP = isCash.UpdateIP,
                            SlipNumber = isCash.SlipNumber,
                            SlipFile = isCash.SlipFile,
                            SlipPath = isCash.SlipPath,
                            EnvironmentID = isCash.EnvironmentID
                        };


                        isCash.IsActive = false;
                        isCash.UpdateDate = DateTime.UtcNow.AddHours(3);
                        isCash.UpdateEmployee = model.Authentication.ActionEmployee.EmployeeID;
                        isCash.UpdateIP = OfficeHelper.GetIPAddress();

                        Db.SaveChanges();

                        //OfficeHelper.AddCashAction(isCash.CashID, isCash.LocationID, null, isCash.ActionTypeID, isCash.Date, isCash.ActionTypeName, isCash.ID, isCash.Date, isCash.DocumentNumber, isCash.Description, -1, 0, -1 * isCash.Amount, isCash.Currency, null, null, isCash.RecordEmployeeID, isCash.RecordDate);

                        result.IsSuccess = true;
                        result.Message = $"{isCash.ID} ID li {isCash.Date} tarihli {isCash.NetAmount} {isCash.Currency} tutarındaki yazarkasa fişi başarı ile iptal edildi";


                        //var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentCashCollections>(self, isCash, OfficeHelper.getIgnorelist());
                        OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Remove", isCash.ID.ToString(), "CashRecorder", "Index", null, true, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);

                    }
                    catch (Exception ex)
                    {

                        result.Message = $"{isCash.NetAmount} {isCash.Currency} tutarındaki yazarkasa fişi iptal edilemedi : {ex.Message}";
                        OfficeHelper.AddApplicationLog("Office", "CashRecorder", "Remove", "-1", "CashRecorder", "Index", null, false, $"{result.Message}", string.Empty, DateTime.UtcNow.AddHours(3), model.Authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);
                    }
                }

            }

            TempData["result"] = result;
            return RedirectToAction("Index", "CashRecorder");

        }
    }
}