using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ActionForce.Entity;

namespace ActionForce.Office
{
    public class HomeControlModel : LayoutControlModel
    {
        public IEnumerable<VLocation> Locations { get; set; }
    }
}