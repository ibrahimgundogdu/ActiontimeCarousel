using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class SaleDayLocationModal
    {
        public int OurCompanyID { get; set; }
        public int LocationID { get; set; }
        public int PayMethodID { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public int StatusID { get; set; }

    }
}
