using ActionForce.Entity;
using ActionForce.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class EnvelopeControlModel : LayoutControlModel
    {
        public DateTime DocumentDate { get; set; }
        public DateTime ProcessDate { get; set; }
        public IEnumerable<VEmployeeCashActions> EmployeeActions { get; set; }
        public IEnumerable<EmployeeShiftModel> EmployeeShifts { get; set; }
    }
}