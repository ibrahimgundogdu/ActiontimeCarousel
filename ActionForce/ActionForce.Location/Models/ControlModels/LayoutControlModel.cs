using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace ActionForce.Location
{
    public class LayoutControlModel
    {
        public AuthenticationModel Authentication { get; set; }
        public LocationScheduleInfo Schedule { get; set; }
        public LocationShiftInfo Shift { get; set; }
        public LocationInfo Location { get; set; }
        public Result Result { get; set; }
        public DateTime LocationDate { get; set; }

        public LayoutControlModel()
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User is GenericPrincipal principal && principal.Identity is FormsIdentity identity)
            {
                Authentication = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>(identity.Ticket.UserData);

                using (ActionTimeEntities db = new ActionTimeEntities())
                {
                    Location = Authentication.CurrentLocation;
                    LocationDate = LocationHelper.GetLocationScheduledDate(Location.ID, DateTime.UtcNow.AddHours(Location.TimeZone)).Date;


                    Schedule = db.LocationSchedule.Where(x => x.LocationID == Location.ID && x.ShiftDate == LocationDate).Select(x => new LocationScheduleInfo()
                    {
                        LocationID = x.LocationID.Value,
                        ScheduleDate = x.ShiftDate.Value,
                        DateStart = x.ShiftDateStart.Value,
                        DateEnd = x.ShiftdateEnd,
                        Duration = x.ShiftDuration
                    }).FirstOrDefault();

                    Shift = db.LocationShift.Where(x => x.LocationID == Location.ID && x.ShiftDate == LocationDate).Select(x => new LocationShiftInfo()
                    {
                        LocationID = x.LocationID,
                        ScheduleDate = x.ShiftDate,
                        DateStart = x.ShiftDateStart.Value,
                        DateEnd = x.ShiftDateFinish,
                        Duration = x.ShiftDuration
                    }).FirstOrDefault();
                }

                Result = new Result() { IsSuccess = false, Message = string.Empty };
            }


        }
    }
}