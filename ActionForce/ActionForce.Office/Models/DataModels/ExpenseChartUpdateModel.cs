using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ExpenseChartUpdateModel
    {
        public long ID { get; set; }
        public double? Amount { get; set; }
        public double? Rate { get; set; }
        public string UpdateDate { get; set; }
        public string UpdateEmployee { get; set; }

        public double? DistRate { get; set; }
        public double? DistAmount { get; set; }

        public double? BalanceRate { get; set; }
        public double? BalanceAmount { get; set; }


    }
}