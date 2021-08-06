using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class Receipt
    {
        public long PaidAmount { get; set; }
        public short ReceiptNo { get; set; }
        public short ZNo { get; set; }
        public short EkuNo { get; set; }
        public string TransDateTime { get; set; }
        public int TicketType { get; set; }
        public Invoice Invoice { get; set; }
        public IEnumerable<Payment> PaymentList { get; set; }
        public IEnumerable<Discount> DiscountList { get; set; }

    }
}