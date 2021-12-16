using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DateTime ProcessDate { get; set; }
        public string ProcessNumber { get; set; }
    }

    public class ParameterResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public DateTime ProcessDate { get; set; }
        public int IsChanged { get; set; } // 0 değişme yok, 1 değişme var

        public int? UnitPrice { get; set; }
        public int? MiliSecond { get; set; }
        public int? ReadCount { get; set; }
        public int? UnitDuration { get; set; }
    }

}