using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class SetupControlModel
    {
        public List<Location> LocationList { get; set; }
        public Location Location { get; set; }
        public string PosTerminalSerial { get; set; }
        public List<DataEmployee> Employees { get; set; }
        public Result Result { get; set; }

    }
}