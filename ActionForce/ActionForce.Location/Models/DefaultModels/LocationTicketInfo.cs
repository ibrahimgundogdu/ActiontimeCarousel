using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class LocationTicketSaleInfo
    {
        public long RowID { get; set; }
        public long SaleID { get; set; }
        public int StatusID { get; set; }
        public string Part { get; set; }
        public int PayMethodID { get; set; }
        public int Unit { get; set; }
        public float Total { get; set; }
        public string Currency { get; set; }
        public DateTime RecordDate { get; set; }
        public bool IsActive { get; set; }
        public Guid? Uid { get; set; }


    }
}