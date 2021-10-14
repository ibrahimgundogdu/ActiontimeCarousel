using ActionForce.Entity;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class DayResultController : BaseController
    {
        public DocumentManager documentManager;

        public ActionResult Index(string id)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            PosManager manager = new PosManager();

            if (TempData["Result"] != null)
            {
                model.Result = TempData["Result"] as Result;
            }

            var location = Db.Location.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID);
            var ResultDate = location.LocalDate.Value;
            if (!string.IsNullOrEmpty(id))
            {
                DateTime.TryParse(id, out ResultDate);
            }


            model.Employees = manager.GetLocationEmployeesToday(model.Authentication.CurrentLocation.ID);
            List<int> employeeIds = model.Employees.Select(x => x.EmployeeID).ToList();
            model.EmployeeSalaries = Db.EmployeeSalary.Where(x => employeeIds.Contains(x.EmployeeID)).OrderByDescending(x => x.DateStart).ToList();

            model.DocumentDate = ResultDate;
            model.CurrentDayResult = Db.DayResult.FirstOrDefault(x => x.Date == model.DocumentDate && x.LocationID == model.Authentication.CurrentLocation.ID);
            model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ProcessDate == model.DocumentDate.Date).ToList();
            model.EmployeeShifts = documentManager.GetEmployeeShifts(model.DocumentDate, model.Authentication.CurrentLocation.ID);
            model.TicketList = manager.GetLocationTicketsToday(model.DocumentDate, location).Where(x => x.StatusID != 4).ToList();
            //model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();
            //model.LocationBalance = manager.GetLocationSaleBalanceToday(model.DocumentDate, location);
            model.Summary = manager.GetLocationSummary(model.DocumentDate, model.Authentication.CurrentEmployee, location);
            model.CashRecordSlip = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == model.DocumentDate).OrderByDescending(x => x.RecordDate).ToList();

            if (model.CurrentDayResult != null)
            {
                model.ResultDocuments = Db.DayResultDocuments.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == model.DocumentDate && x.ResultID == model.CurrentDayResult.ID).ToList();
            }

            model.ResultStates = Db.ResultState.Where(x => x.StateID <= 2).ToList();

            model.Schedule = Db.LocationSchedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ShiftDate == ResultDate).Select(x => new LocationScheduleInfo()
            {
                LocationID = x.LocationID.Value,
                ScheduleDate = x.ShiftDate.Value,
                DateStart = x.ShiftDateStart.Value,
                DateEnd = x.ShiftdateEnd,
                Duration = x.ShiftDuration
            }).FirstOrDefault();

            model.Shift = Db.LocationShift.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ShiftDate == ResultDate).Select(x => new LocationShiftInfo()
            {
                LocationID = x.LocationID,
                ScheduleDate = x.ShiftDate,
                DateStart = x.ShiftDateStart.Value,
                DateEnd = x.ShiftDateFinish,
                Duration = x.ShiftDuration
            }).FirstOrDefault();



            model.TicketSaleRowSummary = Db.VTicketSaleSaleRowSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == ResultDate).ToList();

            List<int> priceCategorieIds = model.TicketSaleRowSummary.Select(x => x.PriceCategoryID.Value).Distinct().ToList();
            List<int> priceIds = model.TicketSaleRowSummary.Select(x => x.PriceID).Distinct().ToList();

            model.Prices = Db.VPrice.Where(x => priceCategorieIds.Contains(x.PriceCategoryID.Value)).ToList();
            model.Prices.AddRange(Db.VPrice.Where(x => priceIds.Contains(x.ID)).ToList());
            model.Prices = model.Prices.Distinct().ToList();

            int[] cashtypes = new int[] { 10, 21, 24, 28 }.ToArray();
            int[] cardtypes = new int[] { 1, 3, 5 }.ToArray();

            model.TicketSalePaymentSummary = Db.VTicketSalePaymentSummary.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == ResultDate).ToList();
            model.CashActions = Db.CashActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ProcessDate == ResultDate && cashtypes.Contains(x.CashActionTypeID.Value)).ToList();
            model.BankActions = Db.BankActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ProcessDate == ResultDate && cardtypes.Contains(x.BankActionTypeID.Value)).ToList();

            return View(model);
        }




        public ActionResult CalculateSalary()
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            DateTime? documentDate = model.DocumentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(3));

            var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == documentDate);

            if (dayresult != null)
            {
                var check = documentManager.CheckSalaryEarn(documentDate, model.Authentication.CurrentLocation.ID);

                if (check)
                {
                    TempData["Result"] = new Result() { IsSuccess = check, Message = check ? "Hesaplandı" : "Hesaplanamadı" };
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddCashRecordSlip(FormCashRecorder cashrecorder)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var cashamount = PosManager.GetStringToAmount(cashrecorder.CashAmount);
            var creditamount = PosManager.GetStringToAmount(cashrecorder.CreditAmount);
            var netamount = PosManager.GetStringToAmount(cashrecorder.NetAmount);
            var totalamount = PosManager.GetStringToAmount(cashrecorder.TotalAmount);
            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(3));
            var processDate = DateTime.UtcNow.AddHours(3);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());

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
                Currency = model.Authentication.CurrentLocation.Currency,
                DocumentDate = documentDate,
                EnvironmentID = 3,
                IsActive = true,
                LocationID = model.Authentication.CurrentLocation.ID,
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

        [HttpPost]
        public ActionResult UpdateCashRecordSlip(FormCashRecorder cashrecorder)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var cashamount = PosManager.GetStringToAmount(cashrecorder.CashAmount);
            var creditamount = PosManager.GetStringToAmount(cashrecorder.CreditAmount);
            var netamount = PosManager.GetStringToAmount(cashrecorder.NetAmount);
            var totalamount = PosManager.GetStringToAmount(cashrecorder.TotalAmount);
            var documentDate = cashrecorder.DocumentDate;
            var processDate = DateTime.UtcNow.AddHours(3);
            var dayResultID = PosManager.GetDayResultID(model.Authentication.CurrentLocation.ID, documentDate, 1, 2, model.Authentication.CurrentEmployee.EmployeeID, "", PosManager.GetIPAddress());

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
                Currency = model.Authentication.CurrentLocation.Currency,
                DocumentDate = documentDate,
                IsActive = cashrecorder.IsActive == 1 ? true : false,
                LocationID = model.Authentication.CurrentLocation.ID,
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

        public PartialViewResult GetCashRecorder(long ID)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            model.CashRecorderSlip = Db.DocumentCashRecorderSlip.FirstOrDefault(x => x.ID == ID);

            return PartialView("_PartialCashRecorder", model);
        }

        public PartialViewResult GetResultDocument(long ID)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            model.ResultDocument = Db.DayResultDocuments.FirstOrDefault(x => x.ID == ID);

            return PartialView("_PartialResultDocument", model);
        }


        [HttpPost]
        public ActionResult AddResultDocument(FormResultDocument document)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var documentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(3));
            var processDate = DateTime.UtcNow.AddHours(3);

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

        [HttpPost]
        public ActionResult SetResultState(FormResultState result)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            var documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = 2,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var processDate = DateTime.UtcNow.AddHours(3);

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
                    dayresult.UpdateIP = PosManager.GetIPAddress();

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
                    ServiceHelper.AddApplicationLog("Location", "Result", "Update", dayresult.ID.ToString(), "Result", "Detail", isequal, true, $"{model.Result.Message}", string.Empty, processDate, model.Authentication.CurrentEmployee.FullName, PosManager.GetIPAddress(), string.Empty, null);
                }

            }
            else
            {
                model.Result.Message = "Günsonu veya durum bilgisi hatalı";
                ServiceHelper.AddApplicationLog("Location", "Result", "Update", result.DayresultID.ToString(), "Result", "Detail", null, true, $"{model.Result.Message}", string.Empty, processDate, model.Authentication.CurrentEmployee.FullName, PosManager.GetIPAddress(), string.Empty, null);
            }

            TempData["result"] = model.Result;

            return RedirectToAction("Index");


        }

        public ActionResult EmployeeShiftEnd(int EmployeeID, string Token)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            var breakresult = EmployeeBreakEnd(EmployeeID, Token);


            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeShiftEnd(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        public ActionResult EmployeeShiftStart(int EmployeeID, string Token)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeShiftStart(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }

        public ActionResult EmployeeBreakEnd(int EmployeeID, string Token)
        {
            DayResultControlModel model = new DayResultControlModel();
            model.Authentication = this.AuthenticationData;

            DateTime processDate = DateTime.UtcNow.AddHours(model.Authentication.CurrentLocation.TimeZone);

            documentManager = new DocumentManager(
               new ProcessEmployee()
               {
                   ID = model.Authentication.CurrentEmployee.EmployeeID,
                   FullName = model.Authentication.CurrentEmployee.FullName
               },
               PosManager.GetIPAddress(),
               new ProcessCompany()
               {
                   ID = model.Authentication.CurrentLocation.OurCompanyID,
                   Name = "UFE GRUP",
                   Currency = "TRL",
                   TimeZone = 3
               }
           );

            var result = documentManager.EmployeeBreakEnd(Token, processDate, model.Authentication.CurrentLocation.ID, EmployeeID);

            model.Result.IsSuccess = result.IsSuccess;
            model.Result.Message = result.Message;

            TempData["Result"] = model.Result;

            return RedirectToAction("Index");
        }
    }
}