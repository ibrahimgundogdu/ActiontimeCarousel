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

        public string DistRate { get; set; }
        public string DistAmount { get; set; }

        public string BalanceRate { get; set; }
        public string BalanceAmount { get; set; }
        public int ShowButton { get; set; } = 0;


    }
}