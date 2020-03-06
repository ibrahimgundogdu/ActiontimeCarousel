using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CUTicketProduct
    {
        public int ID { get; set; }
        public int OurCompanyID { get; set; }
        public int CategoryID { get; set; }
        public int TicketTypeID { get; set; }
        public string ProductName { get; set; }
        public int Unit { get; set; }
        public string IsActive { get; set; }
    }
}