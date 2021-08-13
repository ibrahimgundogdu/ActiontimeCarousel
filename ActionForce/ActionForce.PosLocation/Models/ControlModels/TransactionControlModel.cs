using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class TransactionControlModel : LayoutControlModel
    {
        public TicketSale Order { get; set; }
    }
}