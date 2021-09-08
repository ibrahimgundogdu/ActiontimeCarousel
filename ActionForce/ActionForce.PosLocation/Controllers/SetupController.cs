using System;
using System.Collections.Generic;
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
            model.Result = new Result() {
                IsSuccess = false,
                Message = string.Empty
            };

            //Response.Cookies.Remove("PosLocation");

            //HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

            //if (locationCookie == null)
            //    locationCookie = new HttpCookie("PosLocation");

            //locationCookie.Value = LocationUID.ToString();
            //locationCookie.Expires = DateTime.Now.AddYears(10);

            //Response.SetCookie(locationCookie);

            var posTerminal = Db.VLocationPosTerminal.Where(x => (x.SicilNumber == SicilNumber  || x.SerialNumber == SicilNumber) && x.IsMaster == true && x.IsActive == true).OrderByDescending(x=> x.RecordDate).FirstOrDefault();

            if (posTerminal != null)
            {
                model.Result.IsSuccess = true;
                model.Result.Message = "Lokasyon Bulundu!";
                model.Location = Db.Location.FirstOrDefault(x => x.LocationID == posTerminal.LocationID);
                model.PosTerminalSerial = posTerminal.SicilNumber;
            }
            else
            {
                model.Result.IsSuccess = false;
                model.Result.Message = "Sicil Numarasına Ait Bir Lokasyon Bulunamadı!";
                model.PosTerminalSerial = SicilNumber;
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

                model.Location = Db.Location.FirstOrDefault(x => x.LocationUID.ToString() == LocationUID & x.IsActive == true);

                if (model.Location != null)
                {
                    PosManager manager = new PosManager();

                    model.Employees = manager.GetLocationEmployeesToday(model.Location.LocationID);

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
            PosManager manager = new PosManager();

            var employeecheck = manager.GetLocationEmployeesToday(form.LocationID).Where(x=> x.EmployeeID == form.EmployeeID).FirstOrDefault();
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

                    return RedirectToAction("Index","Default");
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

    }
}