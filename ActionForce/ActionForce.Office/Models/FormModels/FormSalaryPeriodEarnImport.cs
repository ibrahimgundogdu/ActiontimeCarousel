using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormSalaryPeriodEarnImport
    {
        public int SalaryPeriodID { get; set; }
        public HttpPostedFileBase SalaryFile { get; set; }
    }
}