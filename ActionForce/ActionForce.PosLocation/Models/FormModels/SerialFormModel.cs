using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class SerialFormModel
    {
        public int LocationID { get; set; }
        public int Correct { get; set; }
        public int Fail { get; set; }
        public string PosTerminalSerial { get; set; }
    }
}