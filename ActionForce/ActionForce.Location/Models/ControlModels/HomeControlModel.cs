using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class HomeControlModel : LayoutControlModel
    {
        public IEnumerable<VPriceLastList> PriceList { get; set; }
        public VPriceLastList Price { get; set; }
        public VPrice VPrice { get; set; }
        public LocationBalance LocationBalance { get; set; }
        public SummaryControlModel Summary { get; set; }

        public IEnumerable<LocationTicketSaleInfo> TicketList { get; set; }
        public IEnumerable<AnimalCostume> AnimalCostumes { get; set; }
        public IEnumerable<MallMotoColor> MallMotoColor { get; set; }

        public TicketSaleRows SaleRow { get; set; }
        public TicketSale Sale { get; set; }
        public IEnumerable<TicketStatus> Status { get; set; }
        public IEnumerable<PayMethod> PayMethods { get; set; }
        public String EmployeeRecorded { get; set; }
        public String EmployeeUpdated { get; set; }
        public String SaleChannelName { get; set; }


    }
}