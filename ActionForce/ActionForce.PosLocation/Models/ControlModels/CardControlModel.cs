using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class CardControlModel : LayoutControlModel
    {
        public IEnumerable<VProductPriceLastList> PriceList { get; set; }
        public IEnumerable<VTicketBasket> BasketList { get; set; }
        public VProductPriceLastList Price { get; set; }
        public VTicketBasket BasketItem { get; set; }
        public BasketTotal BasketTotal { get; set; }
        public int? EmployeeBasketCount { get; set; }
    }
}