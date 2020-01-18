using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class AuthenticationModel
    {
        //public LocationUser CurrentUser { get; set; }
        public virtual LocationEmployee CurrentEmployee { get; set; }
        public virtual LocationInfo CurrentLocation { get; set; }
        public virtual LocationOurCompany CurrentOurCompany { get; set; }
        public virtual LocationRoleGroup CurrentRoleGroup { get; set; }
        public string Culture { get; set; }
    }
}