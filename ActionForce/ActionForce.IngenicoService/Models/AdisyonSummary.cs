using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class AdisyonSummary
    {
        public string AdisyonNo { get; set; }
        public long AdisyonID { get; set; }
        public string AdisyonName { get; set; }
        public string TableNo { get; set; }
        public long TotalAmount { get; set; }
        public long NetAmount { get; set; }
    }
}