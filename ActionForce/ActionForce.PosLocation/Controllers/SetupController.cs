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

            model.LocationList = Db.Location.Where(x => x.OurCompanyID == 2 & x.IsActive == true).OrderBy(x => x.SortBy).ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult SetLocation(string LocationUID)
        {
            SetupControlModel model = new SetupControlModel();

            Response.Cookies.Remove("PosLocation");

            HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

            if (locationCookie == null)
                locationCookie = new HttpCookie("PosLocation");

            locationCookie.Value = LocationUID.ToString();
            locationCookie.Expires = DateTime.Now.AddYears(10);

            Response.SetCookie(locationCookie);


            return RedirectToAction("Serial");
        }

        public ActionResult Serial()
        {
            SetupControlModel model = new SetupControlModel();

            HttpCookie locationCookie = System.Web.HttpContext.Current.Request.Cookies["PosLocation"];

            if (locationCookie != null && !string.IsNullOrEmpty(locationCookie.Value))
            {
                var LocationUID = locationCookie.Value;

                model.Location = Db.Location.FirstOrDefault(x => x.LocationUID.ToString() == LocationUID & x.IsActive == true);

                if (model.Location != null)
                {
                    var posTerminal = Db.VLocationPosTerminal.FirstOrDefault(x => x.LocationID == model.Location.LocationID && x.IsMaster == true && x.IsActive == true);

                    if (posTerminal != null)
                    {


                        //// bunu kullancı doğrularsa
                        //Response.Cookies.Remove("PosTerminal");

                        //HttpCookie locationPosCookie = System.Web.HttpContext.Current.Request.Cookies["PosTerminal"];

                        //if (locationPosCookie == null)
                        //    locationPosCookie = new HttpCookie("PosTerminal");

                        //locationPosCookie.Value = posTerminal.SerialNumber.ToString();
                        //locationPosCookie.Expires = DateTime.Now.AddYears(10);

                        //Response.SetCookie(locationPosCookie);

                        model.PosTerminalSerial = posTerminal.SerialNumber;

                    }
                }
            }
            else
            {
                return RedirectToAction("Index");
            }



            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult ConfirmSerial(SerialFormModel form)
        {
            SetupControlModel model = new SetupControlModel();

            if (form.Correct == 1)
            {

                Response.Cookies.Remove("PosTerminal");

                HttpCookie locationPosCookie = System.Web.HttpContext.Current.Request.Cookies["PosTerminal"];

                if (locationPosCookie == null)
                    locationPosCookie = new HttpCookie("PosTerminal");

                locationPosCookie.Value = form.PosTerminalSerial;
                locationPosCookie.Expires = DateTime.Now.AddYears(10);

                Response.SetCookie(locationPosCookie);
            }

            if (form.Fail == 1)
            {

                Response.Cookies.Remove("PosTerminal");
                return RedirectToAction("Serial");
            }

            return RedirectToAction("Employee");

        }

        public ActionResult Employee()
        {
            SetupControlModel model = new SetupControlModel();

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
        public ActionResult UserLoginCheck(SerialFormModel form)
        {
            SetupControlModel model = new SetupControlModel();

            if (form.Correct == 1)
            {

                Response.Cookies.Remove("PosTerminal");

                HttpCookie locationPosCookie = System.Web.HttpContext.Current.Request.Cookies["PosTerminal"];

                if (locationPosCookie == null)
                    locationPosCookie = new HttpCookie("PosTerminal");

                locationPosCookie.Value = form.PosTerminalSerial;
                locationPosCookie.Expires = DateTime.Now.AddYears(10);

                Response.SetCookie(locationPosCookie);
            }

            if (form.Fail == 1)
            {

                Response.Cookies.Remove("PosTerminal");
                return RedirectToAction("Serial");
            }

            return RedirectToAction("Employee");

        }


    }
}