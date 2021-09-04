using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PosFormModel
    {
        public int? ID { get; set; }
        public string SicilNumber { get; set; }
        public string SerialNumber { get; set; }
        public string BrandName { get; set; }
        public string ModelName { get; set; }
        public int? BankAccountID { get; set; }
        public string ClientID { get; set; }
        public int? LocationID { get; set; }

    }
}