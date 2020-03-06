using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormLocationPriceCategory
    {
        public int ID { get; set; }
        public int LocationID { get; set; }
        public int PriceCategoryID { get; set; }
        public DateTime StartDate { get; set; }
    }
}