using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnerControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public Partner Partner { get; set; }
        public List<Partner> Partners { get; set; }

        public PartnerUser PartnerUser { get; set; }
        public List<PartnerUser> PartnerUsers { get; set; }

        public PartnerActions PartnerAction { get; set; }
        public List<PartnerActions> PartnerActions { get; set; }

        public Partnership Partnership { get; set; }
        public List<Partnership> Partnerships { get; set; }




    }
}