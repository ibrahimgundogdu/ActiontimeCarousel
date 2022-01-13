using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class CardReader
    {
        public int ID { get; set; }
        public int? OurCompanyID { get; set; }
        public int? LocationID { get; set; }
        public int? LocationTypeID { get; set; }
        public int? LocationPartID { get; set; }
        public string PartName { get; set; }
        public string PartGroupName { get; set; }
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public string Version { get; set; }
        public string IPAddress { get; set; }
        public int? CardReaderTypeID { get; set; }
        public Guid? UID { get; set; }
        public DateTime? StartDate { get; set; }
        public bool? IsActive { get; set; }
    }
}


