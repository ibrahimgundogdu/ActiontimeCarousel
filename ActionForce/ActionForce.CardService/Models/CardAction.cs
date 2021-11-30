using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class CardAction
    {
        public long ID { get; set; }
        public int OurCompanyID { get; set; }
        public long CardID { get; set; }
        public int LocationID { get; set; }
        public int? CustomerID { get; set; }
        public long? ProcessID { get; set; }
        public Guid? ProcessUID { get; set; }
        public short ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public DateTime ActionDate { get; set; }
        public double? CreditCharge { get; set; }
        public double CreditSpend { get; set; }
        public double Credit { get; set; }
        public string Currency { get; set; }
    }
}