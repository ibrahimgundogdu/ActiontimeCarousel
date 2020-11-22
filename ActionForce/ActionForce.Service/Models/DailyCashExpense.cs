using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class DailyCashExpense
    {
        public long ID { get; set; }
        public int OurCompanyID { get; set; }
        public int LocationID { get; set; }
        public DateTime ActionDate { get; set; }
        public double? Amount { get; set; }
        public string Currency { get; set; }
    }
}
