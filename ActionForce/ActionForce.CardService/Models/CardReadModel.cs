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

    public class CardReaderParameterModel
    {
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public string Version { get; set; }
        public int UnitPrice { get; set; }
        public int MiliSecond { get; set; }
        public int ReadCount { get; set; }
        public int UnitDuration { get; set; }

    }
}

//00175D8B;CC:50:E3:17:5D:8B;1;1;1;1;1
//Serino, macadresi, versiyon, ucret, milisaniye, tetik sayısı, bekleme süresi