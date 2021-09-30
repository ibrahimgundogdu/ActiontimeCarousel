using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class SalesControlModel : LayoutControlModel
    {
        public List<VTicketSaleSummary> SaleSummary { get; set; }
        public VTicketSaleSummary TicketSaleSummary { get; set; }
        public TicketSale TicketSale { get; set; }
        public DocumentExpenseSlip ExpenseSlip { get; set; }
        public Customer Customer { get; set; }
        public TicketSalePosReceipt TicketSalePosReceipt { get; set; }
        public List<VTicketSaleRowSummary> TicketSaleRows { get; set; }
        public List<TicketSalePosPayment> TicketSalePosPayment { get; set; }
        public List<VTicketSalePosPaymentSummary> TicketSalePosPaymentSummary { get; set; }
        public List<TicketSalePosStatus> TicketSalePosStatus { get; set; }
        public List<PosPaymentType> PosPaymentType { get; set; }
        public List<PosPaymentSubType> PosPaymentSubType { get; set; }
        public List<Entity.Environment> Environments { get; set; }
        public List<Currency> Currencys { get; set; }
        public List<VTicketSaleDocumentAction> DocumentActions { get; set; }
        public DateTime DocumentDate { get; set; }
        public string DocumentNumbers { get; set; }
        public double PaymentAmount { get; set; }
        public double BalanceAmount { get; set; }

    }
}