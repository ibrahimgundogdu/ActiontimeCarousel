using ActionForce.PosLocation.Models.Dapper;
using ActionForce.PosLocation.Models.DataModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.PosLocation.Controllers
{
    public class SetupController : BaseController
    {

        public ActionResult Index()
        {
            SetupControlModel model = new SetupControlModel();

            Response.Cookies.Remove("PosTerminal");
            Response.Cookies.Remove("PosLocation");
            Response.Cookies.Remove("AuthenticationToken");

            if (TempData["Result"] != null)
            {
                model.Result = (Result)TempData["Result"];
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult SetLocation(string SicilNumber)
        {
            SetupControlModel model = new SetupControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            SicilNumber = SicilNumber.Trim();
            //Response.Cookies.Remove("PosLocation");

            //HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

            //if (locationCookie == null)
            //    locationCookie = new HttpCookie("PosLocation");

            //locationCookie.Value = LocationUID.ToString();
            //locationCookie.Expires = DateTime.Now.AddYears(10);

            //Response.SetCookie(locationCookie);

            var posTerminal = Db.VLocationPosTerminal.Where(x => (x.SicilNumber == SicilNumber || x.SerialNumber == SicilNumber) && x.IsMaster == true && x.IsActive == true).OrderByDescending(x => x.RecordDate).FirstOrDefault();

            if (posTerminal != null)
            {
                model.Result.IsSuccess = true;
                model.Result.Message = "Lokasyon Bulundu!";
                model.Location = Db.Location.FirstOrDefault(x => x.LocationID == posTerminal.LocationID);
                model.PosTerminalSerial = posTerminal.SicilNumber;
                model.Result.IsSuccess = true;
                model.Result.Message = "Sicil Numarasına Ait Bir Lokasyon Bulundu";
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Sicil Numarası veya Ona Ait Bir Lokasyon Bulunamadı!";
                model.PosTerminalSerial = SicilNumber;
                model.Location = new Entity.Location()
                {
                    LocationFullName = "Lokasyon bulunamadı"
                };
            }


            return View(model);
        }

        //public ActionResult Serial()
        //{
        //    SetupControlModel model = new SetupControlModel();
        //    if (TempData["Result"] != null)
        //    {
        //        model.Result = (Result)TempData["Result"];
        //    }

        //    HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

        //    if (locationCookie != null && !string.IsNullOrEmpty(locationCookie.Value))
        //    {
        //        var LocationUID = locationCookie.Value;

        //        model.Location = Db.Location.FirstOrDefault(x => x.LocationUID.ToString() == LocationUID & x.IsActive == true);

        //        if (model.Location != null)
        //        {
        //            var posTerminal = Db.VLocationPosTerminal.FirstOrDefault(x => x.LocationID == model.Location.LocationID && x.IsMaster == true && x.IsActive == true);

        //            if (posTerminal != null)
        //            {


        //                //// bunu kullancı doğrularsa
        //                //Response.Cookies.Remove("PosTerminal");

        //                //HttpCookie locationPosCookie = System.Web.HttpContext.Current.Request.Cookies["PosTerminal"];

        //                //if (locationPosCookie == null)
        //                //    locationPosCookie = new HttpCookie("PosTerminal");

        //                //locationPosCookie.Value = posTerminal.SerialNumber.ToString();
        //                //locationPosCookie.Expires = DateTime.Now.AddYears(10);

        //                //Response.SetCookie(locationPosCookie);

        //                model.PosTerminalSerial = posTerminal.SicilNumber;

        //            }
        //        }
        //    }
        //    else
        //    {
        //        return RedirectToAction("Index");
        //    }



        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult ConfirmSerial(SerialFormModel form)
        {
            SetupControlModel model = new SetupControlModel();

            if (form.Fail == 1)
            {

                Response.Cookies.Remove("PosTerminal");
                Response.Cookies.Remove("PosLocation");
                Response.Cookies.Remove("AuthenticationToken");

                return RedirectToAction("Index");
            }

            var location = Db.Location.FirstOrDefault(x => x.LocationID == form.LocationID);

            if (location != null)
            {
                Response.Cookies.Remove("PosLocation");

                HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

                if (locationCookie == null)
                    locationCookie = new HttpCookie("PosLocation");

                locationCookie.Value = location.LocationUID.ToString();
                locationCookie.Expires = DateTime.Now.AddYears(10);

                Response.SetCookie(locationCookie);
            }


            Response.Cookies.Remove("PosTerminal");

            HttpCookie locationPosCookie = System.Web.HttpContext.Current.Request.Cookies["PosTerminal"];

            if (locationPosCookie == null)
                locationPosCookie = new HttpCookie("PosTerminal");

            locationPosCookie.Value = form.PosTerminalSerial;
            locationPosCookie.Expires = DateTime.Now.AddYears(10);

            Response.SetCookie(locationPosCookie);

            return RedirectToAction("Employee");

        }

        public ActionResult Employee()
        {
            SetupControlModel model = new SetupControlModel();

            if (TempData["Result"] != null)
            {
                model.Result = (Result)TempData["Result"];
            }

            HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

            if (locationCookie != null && !string.IsNullOrEmpty(locationCookie.Value))
            {
                var LocationUID = locationCookie.Value;

                model.Location = Db.Location.FirstOrDefault(x => x.LocationUID.ToString() == LocationUID && x.IsActive == true);

                if (model.Location != null)
                {
                    PosManager manager = new PosManager();

                    model.Employees = manager.GetLocationEmployeesToday(model.Location.LocationID);

                }
                else
                {
                    return RedirectToAction("Index");
                }

            }
            else
            {
                return RedirectToAction("Index");
            }



            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult GetUser(int LocationID, int EmployeeID)
        {
            var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == EmployeeID);
            DataEmployee dataEmployee = new DataEmployee();
            if (employee != null)
            {
                dataEmployee = new DataEmployee()
                {
                    EmployeeFullname = employee.FullName,
                    EmployeeID = EmployeeID,
                    LocationID = LocationID,
                    PhotoFile = employee.FotoFile
                };
            }

            return PartialView("_PartialUserLogin", dataEmployee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult UserLoginCheck(UserLoginFormModel form)
        {
            SetupControlModel model = new SetupControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            PosManager manager = new PosManager();

            var employeecheck = manager.GetLocationEmployeesToday(form.LocationID).Where(x => x.EmployeeID == form.EmployeeID).FirstOrDefault();
            if (employeecheck != null)
            {
                string password = PosManager.makeMD5(form.Password.Trim());
                var employee = Db.Employee.FirstOrDefault(x => x.EmployeeID == form.EmployeeID && x.Username == form.Username.Trim() && x.Password == password);

                if (employee != null)
                {
                    var location = Db.Location.FirstOrDefault(x => x.LocationID == form.LocationID);
                    string AuthenticationToken = $"{location.LocationUID}|{employee.EmployeeUID}|{string.Empty}";

                    Response.Cookies.Remove("AuthenticationToken");

                    HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies["AuthenticationToken"];

                    if (authCookie == null)
                        authCookie = new HttpCookie("AuthenticationToken");

                    authCookie.Value = AuthenticationToken;
                    authCookie.Expires = DateTime.Now.AddYears(10);

                    Response.SetCookie(authCookie);

                    return RedirectToAction("Index", "Default");
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Kullanıcı Bulunamadı";
                }

            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Lokasyonda Tanımlı Böyle Bir Kullanıcı Bulunamadı";
            }

            TempData["Result"] = model.Result;
            return RedirectToAction("Employee");

        }

        public ActionResult Login()
        {
            SetupControlModel model = new SetupControlModel();
            if (TempData["Result"] != null)
            {
                model.Result = (Result)TempData["Result"];
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            SetupControlModel model = new SetupControlModel();

            HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies["AuthenticationToken"];
            authCookie.Expires = DateTime.Now.AddDays(-1);
            Response.SetCookie(authCookie);

            return RedirectToAction("Employee");
        }

        public ActionResult LockMe()
        {
            HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies["AuthenticationToken"];
            authCookie.Expires = DateTime.Now.AddDays(-1);
            Response.SetCookie(authCookie);

            return RedirectToAction("Employee");
        }


        // Document
        public ActionResult Document()
        {
            HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies["AuthenticationToken"];

            if (authCookie != null)
            {
                string[] token = authCookie.Value.Split('|');

                string locUID = token[0];
                string empUID = token[1];

                var rotativa = new Rotativa.ActionAsPdf("DocumentCatalog", new { LocationUID = locUID, EmployeeUID = empUID });
                rotativa.PageOrientation = Rotativa.Options.Orientation.Portrait;
                rotativa.IsLowQuality = false;

                return rotativa;
            }

            return null;
        }

        [AllowAnonymous]
        public ActionResult DocumentCatalog(string LocationUID, string EmployeeUID)
        {
            EnvelopeDataModel model = new EnvelopeDataModel();
            DataService service = new DataService();
            List<EnvelopeCheck> checkList = new List<EnvelopeCheck>();

            model.Location = service.GetLocation(LocationUID);
            model.Employee = service.GetEmployee(EmployeeUID);
            model.ShiftDate = service.GetDateInfo(model.Location.LocalDate.Value.Date);
            model.DayResult = service.GetDayResult(model.Location.LocationID, model.Location.LocalDate.Value.Date);

            //if (model.DayResult == null)
            //{
            //    model.DayResult = service.CreateDayResult(model.Location.LocationID, model.Location.LocalDate.Value.Date);
            //}





            // var documentManager = new DocumentManager(
            //    new ProcessEmployee()
            //    {
            //        ID = model.Authentication.CurrentEmployee.EmployeeID,
            //        FullName = model.Authentication.CurrentEmployee.FullName  //http://localhost:44305/Setup/Document
            //    },
            //    PosManager.GetIPAddress(),
            //    new ProcessCompany()
            //    {
            //        ID = 2,
            //        Name = "UFE GRUP",
            //        Currency = "TRL",
            //        TimeZone = 3
            //    }
            //);

            // PosManager manager = new PosManager();

            // if (TempData["Result"] != null)
            // {
            //     model.Result = TempData["Result"] as Result;
            // }

            // var location = Db.Location.FirstOrDefault(x => x.LocationID == model.Authentication.CurrentLocation.ID);

            // model.DocumentDate = PosManager.GetLocationScheduledDate(model.Authentication.CurrentLocation.ID, DateTime.UtcNow.AddHours(3));
            // model.CurrentDayResult = Db.DayResult.FirstOrDefault(x => x.Date == model.DocumentDate && x.LocationID == model.Authentication.CurrentLocation.ID);
            // model.EmployeeActions = Db.VEmployeeCashActions.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ProcessDate == model.DocumentDate.Date).ToList();
            // model.EmployeeShifts = documentManager.GetEmployeeShifts(model.DocumentDate, model.Authentication.CurrentLocation.ID);
            // //model.TicketList = manager.GetLocationTicketsToday(model.DocumentDate, location).Where(x => x.StatusID != 4).ToList();
            // model.PriceList = Db.GetLocationCurrentPrices(model.Authentication.CurrentLocation.ID).ToList();
            // model.LocationBalance = manager.GetLocationSaleBalanceToday(model.DocumentDate, location);
            // model.Summary = manager.GetLocationSummary(model.DocumentDate, model.Authentication.CurrentEmployee, location);
            // model.CashRecordSlip = Db.DocumentCashRecorderSlip.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == model.DocumentDate).OrderByDescending(x => x.RecordDate).ToList();

            // if (model.CurrentDayResult != null)
            // {
            //     model.ResultDocuments = Db.DayResultDocuments.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.Date == model.DocumentDate && x.ResultID == model.CurrentDayResult.ID).ToList();
            // }

            // model.ResultStates = Db.ResultState.Where(x => x.StateID <= 2).ToList();

            // model.Schedule = Db.LocationSchedule.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ShiftDate == location.LocalDate).Select(x => new LocationScheduleInfo()
            // {
            //     LocationID = x.LocationID.Value,
            //     ScheduleDate = x.ShiftDate.Value,
            //     DateStart = x.ShiftDateStart.Value,
            //     DateEnd = x.ShiftdateEnd,
            //     Duration = x.ShiftDuration
            // }).FirstOrDefault();

            // model.Shift = Db.LocationShift.Where(x => x.LocationID == model.Authentication.CurrentLocation.ID && x.ShiftDate == location.LocalDate).Select(x => new LocationShiftInfo()
            // {
            //     LocationID = x.LocationID,
            //     ScheduleDate = x.ShiftDate,
            //     DateStart = x.ShiftDateStart.Value,
            //     DateEnd = x.ShiftDateFinish,
            //     Duration = x.ShiftDuration
            // }).FirstOrDefault();











            return View();
        }


        [AllowAnonymous]
        public ActionResult UserAuthenticationCheck(Guid? id, Guid? locationId)
        {
            SetupControlModel model = new SetupControlModel();
            model.Result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };
            Response.Cookies.Remove("PosTerminal");
            Response.Cookies.Remove("PosLocation");
            Response.Cookies.Remove("AuthenticationToken");

            if (id != null && locationId != null)
            {
                var employee = Db.Employee.FirstOrDefault(x => x.EmployeeUID == id && x.IsActive == true && x.IsDismissal != true && (x.AreaCategoryID == 1 || x.AreaCategoryID == 3));
                var location = Db.Location.FirstOrDefault(x => x.LocationUID == locationId && x.IsActive == true);


                if (employee != null && location != null)
                {
                    var posterminal = Db.VLocationPosTerminal.FirstOrDefault(x => x.LocationID == location.LocationID && x.IsActive == true && x.IsMaster == true);

                    if (posterminal != null)
                    {
                        //PosTerminal

                        HttpCookie locationPosCookie = System.Web.HttpContext.Current.Request.Cookies["PosTerminal"];

                        if (locationPosCookie == null)
                            locationPosCookie = new HttpCookie("PosTerminal");

                        locationPosCookie.Value = posterminal.SerialNumber.Trim();
                        locationPosCookie.Expires = DateTime.Now.AddDays(1);

                        Response.SetCookie(locationPosCookie);

                        //PosLocation

                        HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

                        if (locationCookie == null)
                            locationCookie = new HttpCookie("PosLocation");

                        locationCookie.Value = location.LocationUID.ToString();
                        locationCookie.Expires = DateTime.Now.AddDays(1);

                        Response.SetCookie(locationCookie);

                        //AuthenticationToken


                        string AuthenticationToken = $"{location.LocationUID}|{employee.EmployeeUID}|{string.Empty}";

                        HttpCookie authCookie = System.Web.HttpContext.Current.Request.Cookies["AuthenticationToken"];

                        if (authCookie == null)
                            authCookie = new HttpCookie("AuthenticationToken");

                        authCookie.Value = AuthenticationToken;
                        authCookie.Expires = DateTime.Now.AddDays(1);

                        Response.SetCookie(authCookie);

                        model.Result.IsSuccess = true;
                        model.Result.Message = "Kullanıcı Girişi Başarılı";

                        return RedirectToAction("Index", "Default");

                    }
                    else
                    {
                        model.Result.IsSuccess = false;
                        model.Result.Message = "Lokasyon Pos Serisi Tanımı Bulunamadı";
                    }
                }
                else
                {
                    model.Result.IsSuccess = false;
                    model.Result.Message = "Kullanıcı veya Lokasyon Bulunamadı";
                }
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Gelen hatalı parametre";
            }

            TempData["Result"] = model.Result;
            return RedirectToAction("Index");

        }
    }
}