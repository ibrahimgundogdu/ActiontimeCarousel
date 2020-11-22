using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class DailyEmployeeSalaryModel
    {
        public long ID { get; set; }
        public int OurCompanyID { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public double? UnitPrice { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime? ShiftDateStart { get; set; }
        public DateTime? ShiftDateEnd { get; set; }
        public int? Duration { get; set; }
        public double? Amount { get; set; }
        public string Currency { get; set; }
    }
}
