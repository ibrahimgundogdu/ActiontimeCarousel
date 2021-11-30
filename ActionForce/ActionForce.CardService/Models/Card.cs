using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class Card
    {
        public long ID { get; set; }
        public int OurCompanyID { get; set; }
        public short CardTypeID { get; set; }
        public string CardNumber { get; set; }
        public DateTime ExpireDate { get; set; }
        public double Credit { get; set; }
        public string Currency { get; set; }
        public int CardStatusID { get; set; }
        public DateTime? RecordDate { get; set; }
        public DateTime? ActivateDate { get; set; }
        public Guid? UID { get; set; }
        public int? EmployeeID { get; set; }
    }
}

