using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class CardReadModel
    {
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public short ProcessType { get; set; }
        public int ProcessNumber { get; set; }
        public short CardType { get; set; }
        public string CardNumber { get; set; }
        public DateTime? ProcessDate { get; set; }
        public double? RidePrice { get; set; }
        public double? CardBlance { get; set; }
    }

    public class CardReadPersonalModel
    {
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public string CardNumber { get; set; }
        public int EmployeeID { get; set; }
    }
}