using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class LocationFilterModel
    {
        public string LocationName { get; set; }
        public string State { get; set; }
        public string TypeName { get; set; }
        public int IsActive { get; set; }
    }
}