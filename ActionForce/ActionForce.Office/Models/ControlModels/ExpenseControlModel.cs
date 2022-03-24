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

        public ExpenseGroup ExpenseGroup { get; set; }
        public List<ExpenseGroup> ExpenseGroups { get; set; }


        public VExpenseDocumentChart ExpenseDocumentChart { get; set; }
        public List<VExpenseDocumentChart> ExpenseDocumentCharts { get; set; }

        public VExpenseDocumentRows ExpenseDocumentRow { get; set; }
        public List<VExpenseDocumentRows> ExpenseDocumentRows { get; set; }

        public List<OurCompany> OurCompanies { get; set; }

        public bool ItemEditable { get; set; } = true;

        public VExpenseActions ExpenseAction { get; set; }
        public List<VExpenseActions> ExpenseActions { get; set; }

        public ExpenseChartGroup ExpenseChartGroup { get; set; }
        public List<ExpenseChartGroup> ExpenseChartGroups { get; set; }
        public List<ExpenseChartGroupItems> ExpenseChartGroupItems { get; set; }
    }
}