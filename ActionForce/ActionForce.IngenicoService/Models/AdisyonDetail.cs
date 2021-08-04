using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class AdisyonDetail
    {
        public string CheckNo { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string AdisyonName { get; set; }
        public int TicketType { get; set; }
        public Invoice InvoiceInfo { get; set; }
        public CurrentAccount CurrentAccountInfo { get; set; }
        public List<SalesItemizerList> SalesItemizerList { get; set; }
        public List<SaleItem> SaleItemList { get; set; }
        public List<Payment> PaymentList { get; set; }
        public List<Discount> DiscountList { get; set; }
        public List<UserMessageList> UserMessageList { get; set; }

    }
}