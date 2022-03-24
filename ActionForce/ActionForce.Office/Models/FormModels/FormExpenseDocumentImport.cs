using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormExpenseDocumentImport
    {
        public string ExpensePeriod { get; set; }
        public HttpPostedFileBase ExpenseFile { get; set; }
    }
}