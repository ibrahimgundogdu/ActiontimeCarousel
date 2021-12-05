using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class EmployeeCard
    {
        public long ID { get; set; }
        public int OurCompanyID { get; set; }
        public int? EmployeeID { get; set; }
        public long CardID { get; set; }
        public short CardTypeID { get; set; }
        public short CardStatusID { get; set; }
        public string CardNumber { get; set; }
        public DateTime? RecordDate { get; set; }
        
    }
}