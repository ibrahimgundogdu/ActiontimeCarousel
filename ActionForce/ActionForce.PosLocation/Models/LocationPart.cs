using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class LocationPart
    {
        public int LocationID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public int? LocationTypeID { get; set; }
        public int PartID { get; set; }
        public string PartName { get; set; }
    }
}