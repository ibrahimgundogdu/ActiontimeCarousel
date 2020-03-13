using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class DefaultControlModel : LayoutControlModel
    {
        public IEnumerable<VPriceLastList> PriceList { get; set; }
        public IEnumerable<VTicketPromotion> PromotionList { get; set; }
        public IEnumerable<VTicketBasket> BasketList { get; set; }
        public VPriceLastList Price { get; set; }
        public VPrice VPrice { get; set; }
        public VTicketPromotion Promotion { get; set; }
        public VTicketBasket BasketItem { get; set; }
        public BasketTotal BasketTotal { get; set; }
        public int? EmployeeBasketCount { get; set; }
        public TicketInfo TicketInfo { get; set; }
    }
}