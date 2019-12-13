using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ResultController : BaseController
    {
        // GET: Result
        [AllowAnonymous]
        public ActionResult Envelope(string date)
        {
            ResultControlModel model = new ResultControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<DayResult> ?? null;
            }

            var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
            var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);

            if (!string.IsNullOrEmpty(date))
            {
                _date = Convert.ToDateTime(date).Date;
                datekey = Db.DateList.FirstOrDefault(x => x.DateKey == _date);
            }

            model.CurrentDate = datekey;
            model.TodayDateCode = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date.ToString("yyyy-MM-dd");
            model.CurrentDateCode = _date.ToString("yyyy-MM-dd");
            model.PrevDateCode = _date.AddDays(-1).Date.ToString("yyyy-MM-dd");
            model.NextDateCode = _date.AddDays(1).Date.ToString("yyyy-MM-dd");

            model.CurrencyList = OfficeHelper.GetCurrency();
            model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);
            model.LocationList = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            model.LocationScheduleList = Db.VLocationSchedule.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ShiftDate == datekey.DateKey && x.StatusID == 2).ToList();
            model.LocationShiftList = Db.VLocationShift.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID && x.ShiftDate == datekey.DateKey).ToList();

            model.DayResultList = Db.VDayResult.Where(x => x.Date == datekey.DateKey).ToList();


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult New(string id, int? locationID, string date)
        {
            ResultControlModel model = new ResultControlModel();

            if ((locationID > 0 && !string.IsNullOrEmpty(date)) || !string.IsNullOrEmpty(id))
            {
                var _date = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;

                DateTime.TryParse(date, out _date);

                var currentResult = Db.VDayResult.FirstOrDefault(x => x.UID.ToString() == id || (x.LocationID == locationID && x.Date == _date));

                if (currentResult != null)
                {
                    // yönlendir
                    return RedirectToAction("Detail", new { id = currentResult.UID.ToString() });
                }
                else
                {
                    // oluştur ve yönlendir.

                    DayResult dayresult = new DayResult();

                    dayresult.Date = _date;
                    dayresult.EnvironmentID = 2;
                    dayresult.IsActive = true;
                    dayresult.IsMobile = false;
                    dayresult.LocationID = locationID.Value;
                    dayresult.RecordDate = DateTime.UtcNow.AddHours(model.Authentication.ActionEmployee.OurCompany.TimeZone.Value).Date;
                    dayresult.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
                    dayresult.RecordIP = OfficeHelper.GetIPAddress();
                    dayresult.StateID = 1;
                    dayresult.StatusID = null;
                    dayresult.UID = Guid.NewGuid();

                    Db.DayResult.Add(dayresult);
                    Db.SaveChanges();

                    // Itemleri ekle
                    var result = OfficeHelper.AddItemsToResultEnvelope(dayresult.ID);







                    return RedirectToAction("Detail", new { id = dayresult.UID.ToString() });
                }

            }
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Detail(string id, int? locationID, string date)
        {
            ResultControlModel model = new ResultControlModel();

            if (TempData["result"] != null)
            {
                model.Result = TempData["result"] as Result<DayResult> ?? null;
            }

            if (locationID > 0 && !string.IsNullOrEmpty(date))
            {
                DateTime? urldate = Convert.ToDateTime(date).Date;
                model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.LocationID == locationID && x.Date == urldate);
                model.DayResult = Db.DayResult.FirstOrDefault(x => x.LocationID == locationID && x.Date == urldate);
            }
            else if (!string.IsNullOrEmpty(id))
            {
                model.CurrentDayResult = Db.VDayResult.FirstOrDefault(x => x.UID.ToString() == id);
                model.DayResult = Db.DayResult.FirstOrDefault(x => x.UID.ToString() == id);
            }

            if (model.DayResult != null)
            {
                model.DayResultItems = Db.DayResultItems.ToList();
                model.DayResultItemList = Db.VDayResultItemList.Where(x => x.ResultID == model.DayResult.ID).ToList();
                model.BankAccountList = Db.VBankAccount.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
                model.DocumentTypes = Db.DocumentType.Where(x => x.IsActive == true).ToList();
                model.CashActionTypes = Db.CashActionType.Where(x => x.IsActive == true).ToList();
                model.BankActionTypes = Db.BankActionType.Where(x => x.IsActive == true).ToList();
                model.CurrentLocation = Db.Location.FirstOrDefault(x => x.LocationID == model.DayResult.LocationID);
                model.Exchanges = Db.VDocumentSaleExchange.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date).ToList();
                model.BankTransfers = Db.VDocumentBankTransfer.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date && x.IsActive == true).ToList();

                var datekey = Db.DateList.FirstOrDefault(x => x.DateKey == model.DayResult.Date);
                model.CurrentDate = datekey;
                model.TodayDateCode = DateTime.UtcNow.AddHours(model.CurrentLocation.Timezone.Value).Date.ToString("yyyy-MM-dd");
                model.CurrentDateCode = datekey.DateKey.ToString("yyyy-MM-dd");
                model.PrevDateCode = datekey.DateKey.AddDays(-1).Date.ToString("yyyy-MM-dd");
                model.NextDateCode = datekey.DateKey.AddDays(1).Date.ToString("yyyy-MM-dd");

                model.CashActions = Db.VCashActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();
                model.BankActions = Db.VBankActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ActionDate == model.DayResult.Date).ToList();
                model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == model.DayResult.LocationID && x.ProcessDate == model.DayResult.Date).ToList();

                model.CashRecorderSlips = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == model.DayResult.LocationID && x.SlipDate == model.DayResult.Date).ToList();
                model.DayResultDocuments = Db.VDayResultDocuments.Where(x => x.LocationID == model.DayResult.LocationID && x.Date == model.DayResult.Date).ToList();

                model.CurrencyList = OfficeHelper.GetCurrency();

                model.CurrentCompany = Db.OurCompany.FirstOrDefault(x => x.CompanyID == model.Authentication.ActionEmployee.OurCompanyID);

            }
            else
            {
                return RedirectToAction("Envelope");
            }

            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        public PartialViewResult AddResultDocument(long? id, HttpPostedFileBase file)
        {
            Result<CashActions> result = new Result<CashActions>()
            {
                IsSuccess = false,
                Message = string.Empty,
                Data = null
            };

            ResultControlModel model = new ResultControlModel();

           

            TempData["result"] = result;

           

            return PartialView("_PartialResultFiles", model);
        }
    }
}