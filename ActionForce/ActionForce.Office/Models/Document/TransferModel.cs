using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class TransferModel
    {
        public DateTime DocumentDate { get; set; }
        public int? FromLocationID { get; set; }
        public int? FromCashID { get; set; }
        public int? FromBankID { get; set; }
        public int? FromEmplID { get; set; }
        public int? FromCustID { get; set; }
        public int? ToLocationID { get; set; }
        public int? ToCashID { get; set; }
        public int? ToBankID { get; set; }
        public int? ToEmplID { get; set; }
        public int? ToCustID { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public int? CarrierEmployeeID { get; set; }
        public string Description { get; set; }
        public Guid UID { get; set; }
    }
}