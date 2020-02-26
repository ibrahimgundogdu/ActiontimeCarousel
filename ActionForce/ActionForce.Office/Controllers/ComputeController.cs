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
            model.WeekLists = Db.DateList.Where(x => x.WeekYear >= 2017 && x.WeekYear <= DateTime.Now.Year && x.DateKey <= datenow).Select(x => new WeekModel()
            {
                WeekKey = x.WeekYear + "-" + x.WeekNumber,
                WeekYear = x.WeekYear.Value,
                WeekNumber = x.WeekNumber.Value
            }).Distinct().OrderByDescending(x => x.WeekKey).ToList();

            return View(model);
        }

        public void ComputeSalaryEarns(int? locationid, string date, string weekcode)
        {
            ComputeControlModel model = new ComputeControlModel();

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            hubContext.Clients.All.AddMessageToPage("Hakediş", "Hesaplama Başladı");

            if (!string.IsNullOrEmpty(date) || !string.IsNullOrEmpty(weekcode) || locationid > 0)
            {

                if (!string.IsNullOrEmpty(date))
                {
                    var _date = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    model.DateLists = Db.DateList.Where(x => x.DateKey == _date).ToList();
                }

                if (!string.IsNullOrEmpty(weekcode) && string.IsNullOrEmpty(date))
                {
                    model.DateLists = Db.DateList.Where(x => x.WeekKey == weekcode).ToList();
                }

                model.Locations = Db.Location.Where(x => x.OurCompanyID == model.Authentication.ActionEmployee.OurCompanyID).ToList();

                if (locationid > 0)
                {
                    model.Locations = model.Locations.Where(x => x.LocationID == locationid).ToList();
                }

                if (model.DateLists.Count() <= 0)
                {
                    var _datenow = DateTime.UtcNow.Date;
                    model.DateLists = Db.DateList.Where(x => x.DateKey == _datenow).ToList();
                }


                // 01 hakedişler tekrar hesaplanır.
                foreach (var location in model.Locations)
                {

                    foreach (var datelist in model.DateLists)
                    {
                        List<int> employeeIds = new List<int>();

                        var employeeschedule = Db.Schedule.Where(x => x.LocationID == locationid && x.ShiftDate == datelist.DateKey).Select(x => x.EmployeeID.Value).ToList();
                        employeeIds.AddRange(employeeschedule.Distinct());
                        var employeeshift = Db.EmployeeShift.Where(x => x.LocationID == locationid && x.ShiftDate == datelist.DateKey).Select(x => x.EmployeeID.Value).ToList();
                        employeeIds.AddRange(employeeshift.Distinct());
                        employeeIds = employeeIds.Distinct().ToList();

                        foreach (var empid in employeeIds)
                        {
                            var employee = Db.Employee.FirstOrDefault(x=> x.EmployeeID == empid);
                            var result = OfficeHelper.ComputeSalaryEarn(empid, datelist.DateKey, location.LocationID, model.Authentication);
                            hubContext.Clients.All.AddMessageToPage("Hakediş", $"{employee.FullName} çalışanı {datelist.DateKey.ToLongDateString()} günü için hakediş hesaplandı : {result}");

                        }

                        hubContext.Clients.All.AddMessageToPage("Hakediş", $"{location?.SortBy?.Trim()} {location.LocationName} lokasyonunda {datelist.DateKey.ToLongDateString()} günü için hakedişler hesaplandı");

                        // 02. hesaplanan hakedişler günsonu dosyasına yazılır
                        var dayresult = Db.DayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == datelist.DateKey && x.IsActive == true);

                        if (dayresult != null)
                        {
                            if (dayresult.StateID == 2 || dayresult.StateID == 3 || dayresult.StateID == 4)
                            {
                                DocumentManager document = new DocumentManager();
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

                }


            }

           
        }

    }
}