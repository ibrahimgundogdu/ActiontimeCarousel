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
        public VLocationSchedule Schedule { get; set; }
        public VLocationShift Shift { get; set; }
        public VLocation Location { get; set; }
        public Result Result { get; set; }

        public LayoutControlModel()
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User is GenericPrincipal principal && principal.Identity is FormsIdentity identity)
            {
                Authentication = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>(identity.Ticket.UserData);

                using (ActionTimeEntities db = new ActionTimeEntities())
                {
                    Location = db.VLocation.FirstOrDefault(x => x.LocationID == Authentication.CurrentLocation.ID);
                    Schedule = db.VLocationSchedule.FirstOrDefault(x => x.LocationID == Location.LocationID && x.ShiftDate == Location.LocalDate);
                    Shift = db.VLocationShift.FirstOrDefault(x => x.LocationID == Location.LocationID && x.ShiftDate == Location.LocalDate);
                }

                Result = new Result() { IsSuccess = false, Message = string.Empty };
            }

            
        }
    }
}