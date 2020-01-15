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

        public LayoutControlModel()
        {
            if (HttpContext.Current.User != null && HttpContext.Current.User is GenericPrincipal principal && principal.Identity is FormsIdentity identity)
            {
                Authentication = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationModel>(identity.Ticket.UserData);
            }
        }
    }
}