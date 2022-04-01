using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnershipFormModel
    {
        public int LocationID { get; set; }
        public int PartnerID { get; set; }
        public Guid PartnerUID { get; set; }
    }
}