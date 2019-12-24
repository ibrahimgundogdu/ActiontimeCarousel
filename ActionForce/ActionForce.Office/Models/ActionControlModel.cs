using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ActionControlModel : LayoutControlModel
    {
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Cash CurrentCash { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<VCashBankActions> ActionList { get; set; }
        public FilterModel Filters { get; set; }
        public IEnumerable<TotalModel> HeaderTotals { get; set; }
        public IEnumerable<TotalModel> FooterTotals { get; set; }
        public IEnumerable<BankAccount> bankAccount { get; set; }
        public IEnumerable<DocumentPrefix> docPrefix { get; set; }
    }
}