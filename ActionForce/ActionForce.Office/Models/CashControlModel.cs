using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CashControlModel : LayoutControlModel
    {
        public IEnumerable<VDocumentCashCollections> CashCollections { get; set; }
        public IEnumerable<VDocumentTicketSales> CashSales { get; set; }
        public IEnumerable<VDocumentSaleExchange> CashSaleExchanges { get; set; }
        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        
        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }

    }
}