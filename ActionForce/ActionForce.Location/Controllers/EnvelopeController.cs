using ActionForce.Entity;
using ActionForce.Integration.UfeService;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class EnvelopeController : BaseController
    {

        private readonly DocumentManager documentManager;
        public EnvelopeController()
        {
            LayoutControlModel model = new LayoutControlModel();

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               LocationHelper.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentOurCompany.ID,
                   Name = model.Authentication.CurrentOurCompany.Name,
                   Currency = model.Authentication.CurrentOurCompany.Currency,
                   TimeZone = model.Authentication.CurrentOurCompany.TimeZone
               }
           );
        }


        // GET: Envelope
        [AllowAnonymous]
        public ActionResult Index()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();
            LocationServiceManager manager = new LocationServiceManager(Db, model.Authentication.CurrentLocation);

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            model.DocumentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            model.CurrentDayResult = Db.DayResult.FirstOrDefault(x => x.Date == model.DocumentDate && x.LocationID == model.Location.ID);
            model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == model.Location.ID && x.ProcessDate == model.DocumentDate.Date).ToList();
            model.EmployeeShifts = documentManager.GetEmployeeShifts(model.DocumentDate, model.Location.ID);
            model.TicketList = manager.GetLocationTicketsToday(model.DocumentDate);
            model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();
            model.LocationBalance = manager.GetLocationSaleBalanceToday(model.DocumentDate);
            model.Summary = manager.GetLocationSummary(model.DocumentDate, model.Authentication.CurrentEmployee);
            model.CashRecordSlip = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == model.Location.ID && x.Date == model.DocumentDate).OrderByDescending(x => x.RecordDate).ToList();
            model.ResultDocuments = Db.DayResultDocuments.Where(x => x.LocationID == model.Location.ID && x.Date == model.DocumentDate && x.ResultID == model.CurrentDayResult.ID).ToList();
            model.ResultStates = Db.ResultState.Where(x => x.StateID <= 2).ToList();
            //List<int?> employeeids = model.EmployeeActions.Select(x => x.EmployeeID).ToList();


            return View(model);
        }

        [AllowAnonymous]
        public ActionResult CalculateSalary()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            DateTime? documentDate = model.DocumentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));

            var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == model.Location.ID && x.Date == documentDate);

            if (dayresult != null)
            {
                var check = documentManager.CheckSalaryEarn(documentDate, model.Location.ID);

                if (check)
                {
                    TempData["Result"] = new Result() { IsSuccess = check, Message = check ? "Hesaplandı" : "Hesaplanamadı" };
                }
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCashRecordSlip(FormCashRecorder cashrecorder)
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            var cashamount = LocationHelper.GetStringToAmount(cashrecorder.CashAmount);
            var creditamount = LocationHelper.GetStringToAmount(cashrecorder.CreditAmount);
            var netamount = LocationHelper.GetStringToAmount(cashrecorder.NetAmount);
            var totalamount = LocationHelper.GetStringToAmount(cashrecorder.TotalAmount);
            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());

            DateTime slipdate = cashrecorder.ReceiptDate.Add(cashrecorder.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (cashrecorder.ReceiptFile != null && cashrecorder.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(cashrecorder.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

                try
                {
                    //Kaydetme
                    cashrecorder.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\CashRecorder";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            CashRecorderModel documentModel = new CashRecorderModel()
            {
                ActionTypeID = 33,
                ActionTypeName = "Yazar Kasa Z Raporu",
                CashAmount = cashamount,
                CreditAmount = creditamount,
                NetAmount = netamount,
                TotalAmount = totalamount,
                Currency = model.Authentication.CurrentOurCompany.Currency,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                IsActive = true,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = Guid.NewGuid(),
                ProcessDate = processDate,
                SlipDate = slipdate,
                SlipNumber = cashrecorder.ReceiptNumber,
                SlipPath = @"\Document\CashRecorder",
                SlipFile = fileName
            };


            var result = documentManager.AddCashRecorder(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCashRecordSlip(FormCashRecorder cashrecorder)
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            var cashamount = LocationHelper.GetStringToAmount(cashrecorder.CashAmount);
            var creditamount = LocationHelper.GetStringToAmount(cashrecorder.CreditAmount);
            var netamount = LocationHelper.GetStringToAmount(cashrecorder.NetAmount);
            var totalamount = LocationHelper.GetStringToAmount(cashrecorder.TotalAmount);
            var documentDate = cashrecorder.DocumentDate;
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var dayResultID = LocationHelper.GetDayResultID(model.Location.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", LocationHelper.GetIPAddress());

            DateTime slipdate = cashrecorder.ReceiptDate.Add(cashrecorder.ReceiptTime.TimeOfDay);
            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (cashrecorder.ReceiptFile != null && cashrecorder.ReceiptFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(cashrecorder.ReceiptFile.FileName);

                string mappath = Server.MapPath("/Documents");

                try
                {
                    //Kaydetme
                    cashrecorder.ReceiptFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\CashRecorder";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            CashRecorderModel documentModel = new CashRecorderModel()
            {
                ActionTypeID = 33,
                ActionTypeName = "Yazar Kasa Z Raporu",
                CashAmount = cashamount,
                CreditAmount = creditamount,
                NetAmount = netamount,
                TotalAmount = totalamount,
                Currency = model.Authentication.CurrentOurCompany.Currency,
                DocumentDate = documentDate,
                IsActive = cashrecorder.IsActive == 1 ? true : false,
                LocationID = model.Location.ID,
                ResultID = dayResultID,
                UID = cashrecorder.UID,
                ProcessDate = processDate,
                SlipDate = slipdate,
                SlipNumber = cashrecorder.ReceiptNumber,
                SlipPath = @"\Document\CashRecorder",
                SlipFile = fileName
            };


            var result = documentManager.UpdateCashRecorder(documentModel);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public PartialViewResult GetCashRecorder(long ID)
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            model.CashRecorderSlip = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.ID == ID);

            return PartialView("_PartialCashRecorder", model);
        }

        [AllowAnonymous]
        public PartialViewResult GetResultDocument(long ID)
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            model.ResultDocument = Db.DayResultDocuments.FirstOrDefault(x => x.ID == ID);

            return PartialView("_PartialResultDocument", model);
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddResultDocument(FormResultDocument document)
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            var documentDate = LocationHelper.GetLocationScheduledDate(model.Location.ID, DateTime.UtcNow.AddHours(model.Location.TimeZone));
            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            string fileName = string.Empty;

            // dosya işlemleri yapılır
            if (document.ResultFile != null && document.ResultFile.ContentLength > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(document.ResultFile.FileName);

                string mappath = Server.MapPath("/Documents");

                try
                {
                    //Kaydetme
                    document.ResultFile.SaveAs(Path.Combine(mappath, fileName));

                    //Kopyalama
                    if (!Request.IsLocal)
                    {
                        string targetPath = @"C:\inetpub\wwwroot\Action\Document\Envelope";
                        string sourcePath = @"C:\inetpub\wwwroot\location\Documents";
                        string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
                        string destFile = System.IO.Path.Combine(targetPath, fileName);
                        System.IO.File.Copy(sourceFile, destFile, true);
                        System.IO.File.Delete(sourceFile);
                    }
                }
                catch (Exception)
                {
                }
            }

            var result = documentManager.AddResultDocument(document.DayResultID, fileName, "/Document/Envelope", 1, string.Empty, processDate);

            TempData["Result"] = new Result() { IsSuccess = result.IsSuccess, Message = result.Message };

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetResultState(FormResultState result)
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);

            if (result.DayresultID > 0 && result.StateID > 0)
            {
                var dayresult = Db.DayResult.FirstOrDefault(x => x.ID == result.DayresultID);

                if (dayresult != null)
                {

                    DayResult self = new DayResult()
                    {
                        Date = dayresult.Date,
                        Description = dayresult.Description,
                        DayResultItemList = dayresult.DayResultItemList,
                        EnvironmentID = dayresult.EnvironmentID,
                        ID = dayresult.ID,
                        RecordDate = dayresult.RecordDate,
                        IsActive = dayresult.IsActive,
                        UID = dayresult.UID,
                        IsMobile = dayresult.IsMobile,
                        Latitude = dayresult.Latitude,
                        LocationID = dayresult.LocationID,
                        Longitude = dayresult.Longitude,
                        RecordEmployeeID = dayresult.RecordEmployeeID,
                        RecordIP = dayresult.RecordIP,
                        StateID = dayresult.StateID,
                        StatusID = dayresult.StatusID,
                        UpdateDate = dayresult.UpdateDate,
                        UpdateEmployeeID = dayresult.UpdateEmployeeID,
                        UpdateIP = dayresult.UpdateIP
                    };

                    dayresult.StateID = result.StateID;
                    dayresult.Description = result.Description;
                    dayresult.UpdateDate = processDate;
                    dayresult.UpdateEmployeeID = model.Authentication.CurrentEmployee.EmployeeID;
                    dayresult.UpdateIP = LocationHelper.GetIPAddress();

                    Db.SaveChanges();



                    model.Result.IsSuccess = true;
                    model.Result.Message = "Günsonu durumu başarı ile güncellendi. ";

                    // eski sisteme uyumlu kayıtlar at.

                    if (result.StateID == 2 || result.StateID == 3 || result.StateID == 4)
                    {
                        var islocal = Request.IsLocal;
                        var updresult = documentManager.CheckResultBackward(dayresult.ID, islocal);
                        model.Result.Message += updresult.Message;
                    }

                    // log at
                    var isequal = ServiceHelper.PublicInstancePropertiesEqual<DayResult>(self, dayresult, ServiceHelper.getIgnorelist());
                    ServiceHelper.AddApplicationLog("Location", "Result", "Update", dayresult.ID.ToString(), "Result", "Detail", isequal, true, $"{model.Result.Message}", string.Empty, processDate, model.Authentication.CurrentEmployee.FullName, LocationHelper.GetIPAddress(), string.Empty, null);
                }

            }
            else
            {
                model.Result.Message = "Günsonu veya durum bilgisi hatalı";
                ServiceHelper.AddApplicationLog("Location", "Result", "Update", result.DayresultID.ToString(), "Result", "Detail", null, true, $"{model.Result.Message}", string.Empty, processDate, model.Authentication.CurrentEmployee.FullName, LocationHelper.GetIPAddress(), string.Empty, null);
            }

            TempData["result"] = model.Result;

            return RedirectToAction("Index");


        }

        [AllowAnonymous]
        public ActionResult LocationShiftStart()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var serviceresult = documentManager.LocationShiftStart(model.Authentication.CurrentEmployee.Token.ToString(), processDate, model.Location.ID);

            TempData["Result"] = new Result() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult?.Message };

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult LocationShiftEnd()
        {
            EnvelopeControlModel model = new EnvelopeControlModel();

            var processDate = DateTime.UtcNow.AddHours(model.Location.TimeZone);
            var serviceresult = documentManager.LocationShiftEnd(model.Authentication.CurrentEmployee.Token.ToString(), processDate, model.Location.ID);

            TempData["Result"] = new Result() { IsSuccess = serviceresult.IsSuccess, Message = serviceresult?.Message };

            return RedirectToAction("Index");
        }
    }
}