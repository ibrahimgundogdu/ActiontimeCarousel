using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class CardLoadInfoModel
    {
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public string CardNumber { get; set; }
        public double? CardBlance { get; set; }
        public int Process { get; set; }
    }
}