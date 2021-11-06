using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class UfePlusCardControlModel : LayoutControlModel
    {
        public VTicketSaleSummary SaleSummary { get; set; }
        public List<VCustomerCard> CustomerCards { get; set; }
        public Customer Customer { get; set; }
        public FormCardFilter CardFilter { get; set; }
        public float CardCreditAmount { get; set; }
    }
}