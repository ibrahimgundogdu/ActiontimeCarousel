using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Office.Controllers
{
    public class ComputeController : BaseController
    {

        // GET: Compute
        public ActionResult Index()
        {
            ComputeControlModel model = new ComputeControlModel();

            //var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            //hubContext.Clients.All.AddMessageToPage("Hakediş", "Hesaplama Başladı");


            DateTime datenow = DateTime.UtcNow.AddHours(3).Date;
            model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();
            var WeekLists = Db.DateList.Where(x => x.WeekYear >= 2017 && x.WeekYear <= DateTime.Now.Year && x.DateKey <= datenow).Select(x=> new { WeekKey = x.WeekKey, WeekYear = x.WeekYear, WeekNumber  = x.WeekNumber} ).Distinct().ToList();
            List<string> weeknumbers = WeekLists.Select(x => x.WeekKey).Distinct().ToList();

            var dayLists = Db.DateList.Where(x => weeknumbers.Contains(x.WeekKey)).Select(x=> new { WeekKey = x.WeekKey, DateKey = x.DateKey} ).ToList();

            model.WeekLists = WeekLists.Select(x => new WeekModel()
            {
                WeekKey = x.WeekYear + "-" + x.WeekNumber,
                WeekYear = x.WeekYear.Value,
                WeekNumber = x.WeekNumber.Value,
                Between = x.WeekKey
            }).Distinct().ToList();

            foreach (var item in model.WeekLists.ToList())
            {
                var firstday = dayLists.Where(x => x.WeekKey == item.WeekKey).OrderBy(x => x.DateKey).FirstOrDefault()?.DateKey.ToShortDateString();
                var lastday = dayLists.Where(x => x.WeekKey == item.WeekKey).OrderByDescending(x => x.DateKey).FirstOrDefault()?.DateKey.ToShortDateString();

                item.Between = $"{firstday} - {lastday}";
            }

            model.WeekLists = model.WeekLists.Distinct();

            model.WeekKey = dayLists.FirstOrDefault(x => x.DateKey == datenow)?.WeekKey;
            return View(model);
        }

        //public void ComputeSalaryEarns(int? locationid, string date, string weekcode)
        //{
        //    ComputeControlModel model = new ComputeControlModel();

        //    var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
        //    hubContext.Clients.All.AddMessageToPage("Hakediş", "Hesaplama Başladı");

        //    if (!string.IsNullOrEmpty(date) || !string.IsNullOrEmpty(weekcode) || locationid > 0)
        //    {

        //        if (!string.IsNullOrEmpty(date))
        //        {
        //            var _date = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        //            model.DateLists = Db.DateList.Where(x => x.DateKey == _date).ToList();
        //        }

        //        if (!string.IsNullOrEmpty(weekcode) && string.IsNullOrEmpty(date))
        //        {
        //            model.DateLists = Db.DateList.Where(x => x.WeekKey == weekcode).ToList();
        //        }

        //        model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

        //        if (locationid > 0)
        //        {
        //            model.Locations = model.Locations.Where(x => x.LocationID == locationid).ToList();
        //        }

        //        if (model.DateLists.Count() <= 0)
        //        {
        //            var _datenow = DateTime.UtcNow.Date;
        //            model.DateLists = Db.DateList.Where(x => x.DateKey == _datenow).ToList();
        //        }


        //        // 01 hakedişler tekrar hesaplanır.
        //        foreach (var location in model.Locations)
        //        {

        //            foreach (var datelist in model.DateLists)
        //            {

        //                var result = OfficeHelper.CheckSalaryEarn(datelist.DateKey, location.LocationID, model.Authentication);

        //                hubContext.Clients.All.AddMessageToPage("Hakediş", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda {datelist.DateKey.ToLongDateString()} günü için hakedişler hesaplandı : {result}");

        //                // 02. hesaplanan hakedişler günsonu dosyasına yazılır
        //                var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == datelist.DateKey && x.IsActive == true);

        //                if (dayresult != null)
        //                {
        //                    if (dayresult.StateID == 2 || dayresult.StateID == 3 || dayresult.StateID == 4)
        //                    {
        //                        DocumentManager document = new DocumentManager();
        //                        var islocal = Request.IsLocal;

        //                        var updresult = document.CheckResultBackward(dayresult.UID.Value, model.Authentication, islocal);

        //                        hubContext.Clients.All.AddMessageToPage("Günsonu", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda {datelist.DateKey.ToLongDateString()} günü için günsonu dosyasına aktarım sonucu : {updresult.IsSuccess} : {updresult.Message}");

        //                    }
        //                }
        //            }

        //            //03. hasılat raporu çalıştırılır.
        //            var datelistone = model.DateLists.FirstOrDefault();
        //            var resultrevenue = Db.ComputeLocationWeekRevenue(datelistone.WeekNumber, datelistone.WeekYear, location.LocationID);

        //            hubContext.Clients.All.AddMessageToPage("Rapor", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda hasılat raporu hesaplandı");

        //        }


        //    }

           
        //}

        public void ComputeSalaryEarns(int? locationid, string date, string weekcode)
        {
            ComputeControlModel model = new ComputeControlModel();

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            hubContext.Clients.All.AddMessageToPage("Hakediş", "Hesaplama Başladı");

            if (!string.IsNullOrEmpty(date) || !string.IsNullOrEmpty(weekcode) || locationid > 0)
            {
                model.DateLists = Db.DateList.Where(x => x.WeekKey == weekcode).ToList();

                if (!string.IsNullOrEmpty(date))
                {
                    var _date = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    model.DateLists = Db.DateList.Where(x => x.DateKey == _date).ToList();
                }


                List<DateTime> datekeylist = model.DateLists.Select(x => x.DateKey).ToList();
                List<int> locationidlist = Db.VLocationSchedule.Where(x => datekeylist.Contains(x.ShiftDate.Value) && x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).Select(x => x.LocationID.Value).Distinct().ToList();

                model.Locations = Db.Location.Where(x => locationidlist.Contains(x.LocationID)).ToList();

                if (locationid > 0 && locationidlist.Contains(locationid ?? 0))
                {
                    model.Locations = model.Locations.Where(x => x.LocationID == locationid).ToList();
                }

                locationidlist = model.Locations.Select(x => x.LocationID).Distinct().ToList();

               var dayResults = Db.DayResult.Where(x => locationidlist.Contains(x.LocationID) && datekeylist.Contains(x.Date) && x.IsActive == true).ToList();

                DocumentManager document = new DocumentManager();

                // 01 hakedişler tekrar hesaplanır.
                int counter = 1;

                foreach (var location in model.Locations)
                {
                    hubContext.Clients.All.AddMessageToPage("Lokasyon", $"{location?.SortBy?.Trim()} {location.LocationName} Sırası :  {counter} / {model.Locations.Count()}");


                    foreach (var datelist in model.DateLists)
                    {

                        var result = OfficeHelper.CheckSalaryEarn(datelist.DateKey, location.LocationID, model.Authentication);

                        hubContext.Clients.All.AddMessageToPage("Hakediş", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda {datelist.DateKey.ToLongDateString()} günü için hakedişler hesaplandı : {result}");

                        // 02. hesaplanan hakedişler günsonu dosyasına yazılır
                        var dayresult = dayResults.FirstOrDefault(x => x.LocationID == location.LocationID && x.Date == datelist.DateKey);

                        if (dayresult != null)
                        {
                            if (dayresult.StateID == 2 || dayresult.StateID == 3 || dayresult.StateID == 4)
                            {
                                
                                var islocal = Request.IsLocal;

                                var updresult = document.CheckResultBackward(dayresult.UID.Value, model.Authentication, islocal);

                                hubContext.Clients.All.AddMessageToPage("Günsonu", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda {datelist.DateKey.ToLongDateString()} günü için günsonu dosyasına aktarım sonucu : {updresult.IsSuccess} : {updresult.Message}");

                            }
                        }
                    }

                    //03. hasılat raporu çalıştırılır.
                    var datelistone = model.DateLists.FirstOrDefault();
                    var resultrevenue = Db.ComputeLocationWeekRevenue(datelistone.WeekNumber, datelistone.WeekYear, location.LocationID);

                    hubContext.Clients.All.AddMessageToPage("Rapor", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda hasılat raporu hesaplandı");

                    counter++;
                }


            }

            hubContext.Clients.All.AddMessageToPage("Hakediş", "Hesaplama Bitti.");

        }


        // 31.12.2021 tarihi için çalışan carisini sıfırla
        [AllowAnonymous]
        public ActionResult EmployeeActionReset()
        {
            ComputeControlModel model = new ComputeControlModel();

            DateTime documentDate = new DateTime(2021,12,31).Date;

            var tempdocument = Db.TempEmployeeActionReset.Where(x=> x.IsSuccess == false).ToList();

            foreach (var item in tempdocument)
            {

                CashSalaryPaymentF cashsalary = new CashSalaryPaymentF()
                {
                    Amount = item.Amount.Value,
                    CategoryID = item.Category.Value,
                    Currency = item.Currency,
                    Description = item.Description,
                    DocumentDate = item.DocumentDate.Value,
                    EmployeeID = item.EmployeeID.Value,
                    FromBankID = item.BankID.Value,
                    LocationID = item.LocationID.Value,
                    SalaryTypeID = item.SalaryTypeID.Value
                };

                var result = AddSalaryPayment(cashsalary);

                if (result.IsSuccess == true)
                {
                    item.IsSuccess = true;
                    item.Message = result.Message;
                }
                else
                {
                    item.Message = result.Message;
                }

                Db.SaveChanges();
            }

           
            return View(model);
        }


        public Result AddSalaryPayment(CashSalaryPaymentF cashsalary)
        {

            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            CashControlModel model = new CashControlModel();

            if (cashsalary != null)
            {
                var actType = Db.CashActionType.FirstOrDefault(x => x.ID == 31); // maaş avans ödemesi

                if (cashsalary.CategoryID == 18)
                {
                    actType = Db.CashActionType.FirstOrDefault(x => x.ID == 38); // set card Ödemesi
                }

                else if (cashsalary.CategoryID == 11)
                {
                    actType = Db.CashActionType.FirstOrDefault(x => x.ID == 47); // Maaş Kesintisi
                }



                var location = Db.Location.FirstOrDefault(x => x.LocationID == cashsalary.LocationID);
                var amount = cashsalary.Amount;
                int timezone = location.Timezone.Value;
                int? frombankID = null;
                int? fromcashID = null;

                if (cashsalary.FromBankID > 0)
                {
                    frombankID = cashsalary.FromBankID;
                }
                else
                {
                    var cash = OfficeHelper.GetCash(cashsalary.LocationID, cashsalary.Currency);
                    fromcashID = cash.ID;
                }

                var docDate = cashsalary.DocumentDate;
               


                var exchange = OfficeHelper.GetExchange(docDate);



                if (amount > 0)
                {
                    SalaryPayment payment = new SalaryPayment();

                    payment.ActinTypeID = actType.ID;
                    payment.ActionTypeName = actType.Name;
                    payment.Currency = cashsalary.Currency;
                    payment.Description = cashsalary.Description;
                    payment.DocumentDate = docDate;
                    payment.EmployeeID = cashsalary.EmployeeID;
                    payment.EnvironmentID = 2;
                    payment.LocationID = location.LocationID;
                    payment.Amount = amount;
                    payment.UID = Guid.NewGuid();
                    payment.TimeZone = timezone;
                    payment.OurCompanyID = location.OurCompanyID;
                    payment.ExchangeRate = payment.Currency == "USD" ? exchange.USDA.Value : payment.Currency == "EUR" ? exchange.EURA.Value : 1;
                    payment.FromBankID = frombankID;
                    payment.FromCashID = fromcashID;
                    payment.SalaryTypeID = cashsalary.SalaryTypeID;
                    payment.CategoryID = cashsalary.CategoryID;

                    DocumentManager documentManager = new DocumentManager();
                    var addresult = documentManager.AddSalaryPayment(payment, model.Authentication);

                    result.IsSuccess = addresult.IsSuccess;
                    result.Message = addresult.Message;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"Tutar 0'dan büyük olmalıdır.";
                }
            }

            return result;

        }



        [AllowAnonymous]
        public ActionResult BankTransferReset()
        {
            ComputeControlModel model = new ComputeControlModel();

            var tempdocument = Db.TempDocumentBankTransfer.Where(x => x.IsSuccess == false).ToList();

            foreach (var item in tempdocument)
            {

                var result = ChangeTransferStatus(item.UID,item.StatusID);

                if (result.IsSuccess == true)
                {
                    item.IsSuccess = true;
                    item.Message = result.Message;
                }
                else
                {
                    item.Message = result.Message;
                }

                Db.SaveChanges();
            }


            return View(model);
        }


        public Result ChangeTransferStatus(Guid? UID, int? StatusID)
        {
            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            BankControlModel model = new BankControlModel();

            var bankTransfer = Db.DocumentBankTransfer.FirstOrDefault(x => x.UID == UID);

            if (bankTransfer != null)
            {
                DocumentManager documentManager = new DocumentManager();
                result = documentManager.EditBankTransferStatus(bankTransfer, model.Authentication, StatusID.Value);
            }

            return result;

        }

    }
}