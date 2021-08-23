using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class BankTransfer
    {
        public long ID { get; set; }
        public Guid UID { get; set; }
        public int ToBankAccountID { get; set; }
        public string BankAccountName { get; set; }
        public string DocumentNumber { get; set; }
        public double? Amount { get; set; }
        public double? Commission { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Date { get; set; }
    }
}