using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class FormResultDocument
    {
        public long? DayResultID { get; set; }
        public HttpPostedFileBase ResultFile { get; set; }
    }
}