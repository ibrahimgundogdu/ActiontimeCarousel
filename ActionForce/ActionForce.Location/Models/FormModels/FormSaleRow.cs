using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class FormSaleRow
    {
        public TimeSpan RecordTime { get; set; }
        public Guid? UID { get; set; }
        public int StatusID { get; set; }
        public int PriceID { get; set; }
        public int? ExtraUnit { get; set; }
        public int PayMethodID { get; set; }
        public int? ColorID { get; set; }
        public int? CostumeID { get; set; }
        public string Description { get; set; }
    }
}