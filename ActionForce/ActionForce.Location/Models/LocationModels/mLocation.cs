using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class mLocation
    {
        public int? LocationID { get; set; }
        public string LocationName { get; set; }
        public string SortBy { get; set; }
        public string TypeName { get; set; }
        public LocationStatus Status { get; set; }
        public string Schedule { get; set; }
        public string Shift { get; set; }
        public string CompanyCode { get; set; }
        public Guid? UID { get; set; }
    }
}