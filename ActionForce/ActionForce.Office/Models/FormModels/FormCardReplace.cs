using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormCardReplace
    {
        public long SaleId { get; set; }
        public long TicketSaleCreditLoadId { get; set; }
        public string CardNumber { get; set; }
        public string NewCardNumber { get; set; }
    }
}