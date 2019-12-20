using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class BankTransfer
    {

        public int ActinTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public double Amount { get; set; }
        public double Commission { get; set; }
        public double? ExchangeRate { get; set; }
        public int LocationID { get; set; }
        public int OurCompanyID { get; set; }
        public int EmployeeID { get; set; }
        public int? FromCashID { get; set; }
        public int? ToBankID { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public int? EnvironmentID { get; set; }
        public int? TimeZone { get; set; }
        public Guid UID { get; set; }
        public string SlipNumber { get; set; }
        public DateTime? SlipDate { get; set; }
        public string SlipPath { get; set; }
        public int StatusID { get; set; }
        public string ReferanceCode { get; set; }
        public string TrackingNumber { get; set; }

    }
}