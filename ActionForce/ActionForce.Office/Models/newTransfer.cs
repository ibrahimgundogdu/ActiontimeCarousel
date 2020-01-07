using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class newTransfer
    {
        public string DocumentDate { get; set; }
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
        public string Amount { get; set; }
        public string Currency { get; set; }
        public int? CarrierEmployeeID { get; set; }
        public string Description { get; set; }
    }

    public class editTransfer
    {
        public string DocumentDate { get; set; }
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

        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExchangeRate { get; set; }

        public int? CarrierEmployeeID { get; set; }
        public string Description { get; set; }

        public int? StatusID { get; set; }
        public string IsActive { get; set; }
        public long ID { get; set; }
        public Guid? UID { get; set; }
    }
}