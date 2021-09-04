using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PosFilterModel
    {
        public string SerialNumber { get; set; }
        public int? BankAccountID { get; set; }
        public int? LocationID { get; set; }
    }
}