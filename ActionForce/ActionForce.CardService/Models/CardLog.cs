using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class CardLog
    {
        public long ID { get; set; }
        public string Message { get; set; }
        public string RecordIP { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Module { get; set; }
        public string ResponseMessage { get; set; }
        public string RecordDate { get; set; }
    }
}
