using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CUPriceCategory
    {
        public int ID { get; set; }
        public int OurCompanyID { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string IsMaster { get; set; }
        public string SortBy { get; set; }
        public string IsActive { get; set; }
    }
}