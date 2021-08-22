using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class LocationControlModel : LayoutControlModel
    {
        public LocationScheduleInfo Schedule { get; set; }
        public LocationShiftInfo Shift { get; set; }
    }
}