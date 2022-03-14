using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ExpenseFormModel
    {
        public long ExpenseDocumentID { get; set; }
        public DateTime DocumentDate { get; set; }
        public string StatusName { get; set; }
        public string DocumentSource { get; set; }
        public int ExpenseCenterID { get; set; }
        public short ExpenseItemID { get; set; }
        public short ExpenseGroupID { get; set; }
        public DateTime ExpensePeriod { get; set; }
        public string TotalAmount { get; set; }
        public string DistributionAmount { get; set; }
        public string TaxRate { get; set; }
        public string Currency { get; set; }
        public string ExpenseDescription { get; set; }
        public Guid UID { get; set; }

    }
}