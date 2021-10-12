using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class CurrentLocation
    {
        public int ID { get; set; }
        public int OurCompanyID { get; set; }
        public int LocationTypeID { get; set; }
        public string FullName { get; set; }
        public Guid? UID { get; set; }
        public int TimeZone { get; set; }
        public string Currency { get; set; }
    }
}