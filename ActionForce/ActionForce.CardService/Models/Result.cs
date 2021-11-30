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
}