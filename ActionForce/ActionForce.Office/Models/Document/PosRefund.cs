using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PosRefund
    {
        public int ActinTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public int? ToCustomerID { get; set; }
        public int? FromBankAccountID { get; set; }
        public int LocationID { get; set; }
        public int OurCompanyID { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public double? ExchangeRate { get; set; }
        public int? EnvironmentID { get; set; }
        public int? TimeZone { get; set; }
        public long? ReferanceID { get; set; }
        public string TerminalID { get; set; }
        public long? ResultID { get; set; }
        public Guid? UID { get; set; }
    }
}