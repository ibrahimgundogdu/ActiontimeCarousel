using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ActionForce.Entity;

namespace ActionForce.Office
{
    public class LocationControlModel : LayoutControlModel
    {
        public Result Result { get; set; }

        public List<GetLocationAll_Result> LocationList { get; set; }
        public List<string> StateList { get; set; }
        public List<string> TypeList { get; set; }
    }
}