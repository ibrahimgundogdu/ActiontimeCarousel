using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class LocationControlModel : LayoutControlModel
    {
        public IEnumerable<mLocation> Locations { get; set; }
        public Result Result { get; set; }
    }
}