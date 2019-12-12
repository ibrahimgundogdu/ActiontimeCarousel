using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CashActionModel
    {
        public long ID { get; set; }
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public string ProcessName { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
        public string Document { get; set; }
        public string Description { get; set; }
        public DateTime RecordDate { get; set; }
    }

}