using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService
{
    public class CardDeposite
    {
        public long ID { get; set; }
        public long SaleID { get; set; }
        public DateTime Date { get; set; }
        public Guid? UID { get; set; }
    }
}