using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class ConfirmFormModel
    {
        public long OrderID { get; set; }
        public long SlipID { get; set; }
        public string SMSCode { get; set; }
    }
}