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

        public PartnerFilterModel Filters { get; set; }

        public PartnerUser PartnerUser { get; set; }
        public List<PartnerUser> PartnerUsers { get; set; }

        public VPartnerActions PartnerAction { get; set; }
        public List<VPartnerActions> PartnerActions { get; set; }
        public List<VPartnerActions> OldPartnerActions { get; set; }

        public VPartnership Partnership { get; set; }
        public List<VPartnership> Partnerships { get; set; }

        public Location Location { get; set; }
        public List<Location> Locations { get; set; }

        public ExpensePeriod ExpensePeriod { get; set; }
        public List<ExpensePeriod> ExpensePeriods { get; set; }

        public VDocumentPartnerEarn DocumentPartnerEarn { get; set; }
        public List<VDocumentPartnerEarn> DocumentPartnerEarns { get; set; }
        public List<VDocumentPartnerEarnRow> DocumentPartnerEarnRows { get; set; }

        public VDocumentPartnerPayment DocumentPartnerPayment { get; set; }
        public List<VDocumentPartnerPayment> DocumentPartnerPayments { get; set; }
        public List<PayMethod> PayMethods { get; set; }



    }
}