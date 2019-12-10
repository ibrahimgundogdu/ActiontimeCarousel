using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CashCollection
    {
        public int ActinTypeID { get; set; }
        public int? FromEmployeeID { get; set; }
        public int? FromBankAccountID { get; set; }
        public int? FromCustomerID { get; set; }
        public int LocationID { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public double? ExchangeRate { get; set; }
        public int? EnvironmentID { get; set; }
        public Guid? UID { get; set; }
    }
}