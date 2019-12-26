using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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

            
            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            //model.CurrentLocation = Db.VLocation.FirstOrDefault(x => x.LocationID == model.Filters.LocationID);

            model.Detail = Db.VDocumentCashRecorderSlip.FirstOrDefault(x => x.UID == id);

            model.History = Db.ApplicationLog.Where(x => x.Controller == "CashRecorder" && x.ProcessID == model.Detail.ID.ToString()).ToList();


            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult AddCashRecorder(NewCashRecorder cashRecord, HttpPostedFileBase documentFile)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
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
                var netamount = Convert.ToDouble(cashRecord.NetAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var totalamount = Convert.ToDouble(cashRecord.TotalAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
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

                var resultID = Db.DayResult.FirstOrDefault(x => x.LocationID == cashRecord.LocationID && x.Date == docDate)?.ID;
                DateTime sDatetime = slipDate.Add(Convert.ToDateTime(cashRecord.SlipTime).TimeOfDay);

                var cash = OfficeHelper.GetCash(cashRecord.LocationID, cashRecord.Currency);

                var exchange = OfficeHelper.GetExchange(docDate);

                string path = Server.MapPath("/");

                if (netamount > 0)
                {
                    CashRecorder record = new CashRecorder();

                    record.ActinTypeID = actType.ID;
                    record.ActionTypeName = actType.Name;
                    record.NetAmount = netamount;
                    record.TotalAmount = totalamount;
                    record.Currency = currency;
                    record.DocumentDate = docDate;
                    record.LocationID = cashRecord.LocationID;
                    record.OurCompanyID = location.OurCompanyID;
                    record.TimeZone = timezone;
                    record.SlipNumber = cashRecord.SlipNumber;
                    record.SlipDate = sDatetime;
                    record.ResultID = resultID;

                    record.SlipFile = "";
                    record.SlipPath = "";

                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        record.SlipFile = filename;
                        record.SlipPath = "/Document/CashRecord";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(record.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.AddCashRecorder(record, model.Authentication);
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

            return RedirectToAction("Index", "CashRecorder");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult EditCashRecorder(NewCashRecorder cashRecord, HttpPostedFileBase documentFile)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
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
                var netamount = Convert.ToDouble(cashRecord.NetAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var totalamount = Convert.ToDouble(cashRecord.TotalAmount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
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


                DateTime sDatetime = slipDate.Add(Convert.ToDateTime(cashRecord.SlipTime).TimeOfDay);

                var cash = OfficeHelper.GetCash(cashRecord.LocationID, cashRecord.Currency);

                var exchange = OfficeHelper.GetExchange(docDate);

                string path = Server.MapPath("/");

                if (netamount > 0)
                {
                    CashRecorder record = new CashRecorder();

                    record.NetAmount = netamount;
                    record.TotalAmount = totalamount;
                    record.Currency = currency;
                    record.DocumentDate = docDate;
                    record.LocationID = cashRecord.LocationID;
                    record.TimeZone = timezone;
                    record.SlipNumber = cashRecord.SlipNumber;
                    record.SlipDate = sDatetime;
                    record.UID = cashRecord.UID;

                    //record.SlipPath = "";
                    //record.SlipFile = "";

                    if (documentFile != null && documentFile.ContentLength > 0)
                    {
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(documentFile.FileName);
                        record.SlipFile = filename;
                        record.SlipPath = "/Document/CashRecorder";

                        try
                        {
                            documentFile.SaveAs(Path.Combine(Server.MapPath(record.SlipPath), filename));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    DocumentManager documentManager = new DocumentManager();
                    result = documentManager.EditCashRecorder(record, model.Authentication);
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
            return RedirectToAction("Detail", new { id = cashRecord.UID });

        }

        [AllowAnonymous]
        public ActionResult DeleteCashRecorder(int? id)
        {
            Result<DocumentCashRecorderSlip> result = new Result<DocumentCashRecorderSlip>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };
            CashControlModel model = new CashControlModel();


            if (id != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.DeleteCashRecorder(id, model.Authentication);
            }

            Result<CashActions> messageresult = new Result<CashActions>();
            messageresult.Message = result.Message;

            TempData["result"] = messageresult;
            return RedirectToAction("Index", "CashRecorder");

        }
    }
}