using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class CardReaderAction
    {
        public long ID { get; set; }
        public int CardReaderID { get; set; }
        public Guid CardReaderUID { get; set; }
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public short ProcessType { get; set; }
        public short ProcessNumber { get; set; }
        public short CardType { get; set; }
        public string CardNumber { get; set; }
        public DateTime ProcessDate { get; set; }
        public double RideAmount { get; set; }
        public double CardBalanceAmount { get; set; }
        public int? LocationID { get; set; }
        public Guid? UID { get; set; }
        public DateTime RecordDate { get; set; }

    }
}

