using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class ResultSummary
    {
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public IEnumerable<AdisyonSummary> SummaryList { get; set; }
    }
}