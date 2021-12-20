using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class AuthenticationModel
    {
        public virtual CurrentEmployee CurrentEmployee { get; set; }
        public virtual LockedEmployee LockedEmployee { get; set; }
        public virtual CurrentLocation CurrentLocation { get; set; }
        public bool IsCardSystem { get; set; } = false;
    }
}