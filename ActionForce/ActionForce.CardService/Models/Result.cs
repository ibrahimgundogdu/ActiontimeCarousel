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
        public bool IsAnyChange { get; set; }


        public string Version { get; set; }
        public double? UnitPrice { get; set; }
        public int MiliSecond { get; set; }
        public int ReadCount { get; set; }
        public int UnitDuration { get; set; }
    }

}