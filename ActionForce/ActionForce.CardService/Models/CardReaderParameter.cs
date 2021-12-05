using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class CardReaderParameter
    {
        public long ID { get; set; }
        public int CardReaderID { get; set; }
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public DateTime? StartDate { get; set; }
        public string Version { get; set; }
        public double? UnitPrice { get; set; }
        public int MiliSecond { get; set; }
        public int ReadCount { get; set; }
        public int UnitDuration { get; set; }

    }
}