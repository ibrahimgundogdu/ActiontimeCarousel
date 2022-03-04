using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ExpenseControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public ExpenseFilterModel Filters { get; set; }

        public VExpenseDocument ExpenseDocument { get; set; }
        public List<VExpenseDocument> ExpenseDocuments { get; set; }

        public ExpenseCenter ExpenseCenter { get; set; }
        public List<ExpenseCenter> ExpenseCenters { get; set; }

        public ExpenseItem ExpenseItem { get; set; }
        public List<ExpenseItem> ExpenseItems { get; set; }

        public ExpenseDocumentStatus ExpenseDocumentStatus { get; set; }
        public List<ExpenseDocumentStatus> ExpenseDocumentStatuses { get; set; }

        public ExpensePeriod ExpensePeriod { get; set; }
        public List<ExpensePeriod> ExpensePeriods { get; set; }
    }
}