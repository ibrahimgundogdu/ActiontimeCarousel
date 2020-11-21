using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class SaleDayTotalModal
    {
        public int PaymethodID { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }
}
