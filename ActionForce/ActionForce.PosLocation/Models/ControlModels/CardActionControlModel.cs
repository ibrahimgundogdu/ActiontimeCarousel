using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class CardActionControlModel : LayoutControlModel
    {
        public TicketSale TicketSale { get; set; }
        public List<TicketSaleRows> TicketSaleRow { get; set; }
        public List<TicketSalePosPayment> TicketSalePosPayments { get; set; }
        public Card Card { get; set; }
        public CardReader CardReader { get; set; }
        public TicketSaleCreditLoad CreditLoad { get; set; }
        public double CardBalance { get; set; } = 0;
        public double CardBalanceAction { get; set; } = 0;
        public double CreditPaymentAmount { get; set; } = 0;
        public int MasterCredit { get; set; } = 0;
        public int PromoCredit { get; set; } = 0;
        public int TotalCredit { get; set; } = 0;
        public string CardNumber { get; set; }

    }
}