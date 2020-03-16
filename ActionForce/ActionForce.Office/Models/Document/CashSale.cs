using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CashSale
    {
        public int ActinTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public int? CashID { get; set; }
        public int? FromCustomerID { get; set; }
        public int? PayMethodID { get; set; }
        public int LocationID { get; set; }
        public int OurCompanyID { get; set; }
        public double Amount { get; set; }
        public int? Quantity { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public double? ExchangeRate { get; set; }
        public int? EnvironmentID { get; set; }
        public int? TimeZone { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public Guid? UID { get; set; }
        public bool IsActive { get; set; }
    }
}