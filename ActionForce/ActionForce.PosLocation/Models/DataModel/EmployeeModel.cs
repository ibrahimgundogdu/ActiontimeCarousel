using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class EmployeeModel
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string FotoFile { get; set; }
        public Guid? Token { get; set; }
        public EmployeeShiftLocationModel Shift { get; set; }
        public EmployeeScheduleModel Schedule { get; set; }
        public IEnumerable<EmployeeBreakModel> Breaks { get; set; }
    }
}