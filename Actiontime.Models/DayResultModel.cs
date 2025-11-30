using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class DayResultModel
    {
        public DayResult DayResult { get; set; }
        public CashSummary CashSummary { get; set; }
        public List<TicketSaleSummary> SaleSummaries { get; set; }
        public LocationScheduleShift LocationShift { get; set; }
        public List<EmployeeScheduleShift> employeeShifts { get; set; }

    }
}
