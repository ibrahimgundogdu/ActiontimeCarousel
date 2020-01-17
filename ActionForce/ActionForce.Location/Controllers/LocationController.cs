using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ActionForce.Location.Controllers
{
    public class LocationController : BaseController
    {
        // GET: Location
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SetCurrentLocation()
        {
            LocationControlModel model = new LocationControlModel();

            List<mLocation> mLocations = new List<mLocation>();

            List<int> locationisd = Db.EmployeeLocation.Where(x => x.EmployeeID == model.Authentication.CurrentUser.CurrentEmployee.EmployeeID).Select(x=> x.LocationID).ToList();

            var Locations = Db.Location.Where(x => locationisd.Contains(x.LocationID) && x.IsActive == true).ToList();
            var ourcompanies = Db.OurCompany.ToList();
            var locationtypes = Db.LocationType.ToList();


            foreach (var location in Locations)
            {
                var locationtype = locationtypes.FirstOrDefault(x => x.ID == location.LocationTypeID);
                var ourcompany = ourcompanies.FirstOrDefault(x => x.CompanyID == location.OurCompanyID);

                //var schedule = Db.GetLocationCurrentSchedule(location.LocationID, location.LocalDate).Select(x=> new mLocationSchedule() {
                //    LocationID = x.LocationID.Value,
                //    DurationMinute = x.DurationMinute,
                //    ShiftDate = x.ShiftDate,
                //    ShiftStart = x.ShiftDateStart,
                //    ShiftEnd = x.ShiftdateEnd
                //}).FirstOrDefault();

                var shift = Db.GetLocationCurrentShift(location.LocationID, location.LocalDate).Select(x => new mLocationShift()
                {
                    LocationID = x.LocationID,
                    DurationMinute = x.DurationMinute,
                    ShiftDate = x.ShiftDate,
                    ShiftStart = x.ShiftDateStart,
                    ShiftEnd = x.ShiftDateFinish
                }).FirstOrDefault();

                mLocations.Add(new mLocation()
                {
                    LocationID = location.LocationID,
                    LocationName = $"{location.LocationName} {location.Description} {location.State}",
                    SortBy = location.SortBy,
                    TypeName = locationtype.TypeName ?? location.Description,
                    //Schedule = schedule != null ? $"{schedule.ShiftStart?.ToString("HH\\:mm")} - {schedule.ShiftEnd?.ToString("HH\\:mm")}" :"",
                    //Shift = shift != null ? $"{shift.ShiftStart?.ToString("HH\\:mm")} - {shift.ShiftEnd?.ToString("HH\\:mm")}" : "",
                    Status = shift == null ? LocationStatus.Beklemede : shift != null && shift.DurationMinute > 0 ? LocationStatus.Kapalı : LocationStatus.Açık,
                    CompanyCode = ourcompany.Code,
                    UID = location.LocationUID
                });
            }

            model.Locations = mLocations;

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult InitCurrentLocation(Guid? id)
        {

            if (id != null)
            {
                return RedirectToAction("Index", "Default");
            }
            else
            {
                return RedirectToAction("SetCurrentLocation","Location");
            }
            var location = Db.Location.FirstOrDefault(x => x.LocationUID == id);

            

            return View();
        }
    }
}