using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CashExpense
    {
        public int ActinTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public int? CashID { get; set; }
        public int? ExpenseTypeID { get; set; }
        public int? ToEmployeeID { get; set; }
        public int? ToBankAccountID { get; set; }
        public int? ToCustomerID { get; set; }
        public int LocationID { get; set; }
        public int OurCompanyID { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public double? ExchangeRate { get; set; }
        public int? EnvironmentID { get; set; }
        public string SlipNumber { get; set; }
        public string SlipDocument { get; set; }
        public string ReferanceModel { get; set; }
        public int? TimeZone { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public Guid? UID { get; set; }
        public string SlipPath { get; set; }
        public DateTime? SlipDate { get; set; }


    }
}