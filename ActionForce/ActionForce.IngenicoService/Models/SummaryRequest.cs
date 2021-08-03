using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class SummaryRequest
    {
        public Header Header_Info { get; set; }
        public string AdisyonNo { get; set; }
        public string SerialNo { get; set; }
    }
}