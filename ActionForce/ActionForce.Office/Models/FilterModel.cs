using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FilterModel
    {
        public int? LocationID { get; set; }
        public int? EmployeeID { get; set; }
        public int? BankAccountID { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
    }

    public class ResultFilterModel
    {
        public int? LocationID { get; set; }
        public int? BankAccountID { get; set; }
        public DateTime? ResultDate { get; set; }
        public long? ResultID { get; set; }
    }
}