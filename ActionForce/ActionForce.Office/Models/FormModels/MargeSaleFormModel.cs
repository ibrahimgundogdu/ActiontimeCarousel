using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class MargeSaleFormModel
    {
        public long LoadedSaleID { get; set; }
        public string PaymentSaleNumber { get; set; }
        public Guid TicketSaleUID { get; set; }

    }
}