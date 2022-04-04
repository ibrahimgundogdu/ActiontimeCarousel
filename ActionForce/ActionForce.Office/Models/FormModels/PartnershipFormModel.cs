using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnershipFormModel
    {
        public int PartnershipID { get; set; }
        public int UFEPartnershipID { get; set; }
        
        public int LocationID { get; set; }
        public int PartnerID { get; set; }
        public Guid PartnerUID { get; set; }
        public Guid LocationUID { get; set; }
        public int PartnershipRate { get; set; }
        public int UFEPartnershipRate { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string IsActive { get; set; }
        public string SubmitUpdate { get; set; }
        public string SubmitAdd { get; set; }
        public string SubmitEnd { get; set; }



    }
}