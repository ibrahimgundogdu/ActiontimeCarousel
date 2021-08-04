using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class ResultDetail
    {
        public int ResultCode { get; set; }
        public string ResultMessage { get; set; }
        public AdisyonDetail Detail { get; set; }
        public int TicketTypeChangeDisabled { get; set; }

    }
}