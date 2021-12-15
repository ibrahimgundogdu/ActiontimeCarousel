using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class CardCreditLoad
    {
        public long ID { get; set; }
        public int OurCompanyID { get; set; }
        public int LocationID { get; set; }
        public long SaleID { get; set; }
        public bool FromCardReader { get; set; }
        public string CardNumber { get; set; }
        public double? ExistsCredit { get; set; }
        public double? MasterCredit { get; set; }
        public double? PromoCredit { get; set; }
        public double? TotalCredit { get; set; }
        public double? FinalCredit { get; set; }
        public double? PaymentAmount { get; set; }
        public string Currency { get; set; }
        public DateTime PaymentDate { get; set; }
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public DateTime RecordDate { get; set; }
        public int RecordEmployeeID { get; set; }
        public string RecordIP { get; set; }
        public Guid UID { get; set; }
        public bool IsSuccess { get; set; }

    }
}

