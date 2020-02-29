using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PriceFilterModel
    {
        public bool? IsActive { get; set; }
        public int? OurCompanyID { get; set; }
        public int? TicketTypeID { get; set; }
        public int? PriceCategoryID { get; set; }
        public int? ProductID { get; set; }
        public string Active { get; set; }
        public int? ListType { get; set; }
    }
}