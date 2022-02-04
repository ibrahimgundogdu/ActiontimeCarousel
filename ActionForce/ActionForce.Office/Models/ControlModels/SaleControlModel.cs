using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class SaleControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public VPriceCategory PriceCategory { get; set; }
        public List<VPriceCategory> PriceCategoryList { get; set; }
        public List<VPriceLastList> PriceLastList { get; set; }
        public List<VPrice> PriceList { get; set; }
        public List<Tax> TaxList { get; set; }
        public VPrice Price { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
        public List<TicketProductCategory> TicketProductCategoryList { get; set; }
        public List<TicketType> TicketTypeList { get; set; }
        public List<VTicketProduct> TicketProductList { get; set; }
        public VTicketProduct TicketProduct { get; set; }
        public PriceFilterModel FilterModel { get; set; }

        public VPriceCategory CurrentPriceCategory { get; set; }
        public TicketType CurrentTicketType { get; set; }

        public FilterModel Filters { get; set; }
        public Location CurrentLocation { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public List<VTicketSaleAllSummary> TicketSaleSummary { get; set; }
        public VTicketSaleAllSummary CurrentTicketSaleSummary { get; set; }

        public TicketSale TicketSale { get; set; }

        public List<VTicketSaleRowSummary> TicketSaleRows { get; set; }
        public List<VTicketSaleSaleRowSummary> TicketSaleSaleRows { get; set; }
        public List<VTicketSalePosPaymentSummary> TicketSalePosPaymentSummary { get; set; }
        public List<VDocumentsAllSummaryUnion> DocumentsAllSummary { get; set; }
        public List<VSaleActionsSummaryUnion> SaleActionsAllSummary { get; set; }

        public List<VDocumentExpenseSlip> DocumentExpenseSlips { get; set; }

        public List<HCardActions> CardActions { get; set; }

        public List<Employee> Employees { get; set; }

        public List<LocationPart> LocationParts { get; set; }
        public List<GetSaledUsedStats_Result> GetSaledUsedStats { get; set; }
        public double DepositeUnitPrice { get; set; }

    }
}