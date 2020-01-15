using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class AuthenticationModel
    {
        public LocationUser CurrentUser { get; set; }
        public string Culture { get; set; }
    }
}