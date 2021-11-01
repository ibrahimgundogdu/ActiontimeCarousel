using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class FormCardFilter
    {
        public string QRCode { get; set; }
        public string CardNumber { get; set; }
        public string PhoneNumber { get; set; }
        public long OrderID { get; set; }

    }
}