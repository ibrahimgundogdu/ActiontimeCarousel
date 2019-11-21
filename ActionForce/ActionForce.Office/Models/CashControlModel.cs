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
        public IEnumerable<VDocumentCashOpen> CashOpenSlip { get; set; }

        public IEnumerable<VDocumentCashPayments> CashPayments { get; set; }
        public IEnumerable<VDocumentTicketSaleReturn> TicketSalesReturn { get; set; }
        public IEnumerable<VDocumentCashExpense> CashExpense { get; set; }
        public IEnumerable<VDocumentBankTransfer> BankTransfer { get; set; }
        public IEnumerable<VDocumentSalaryPayment> SalaryPayment { get; set; }

        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<FromAccountModel> ToList { get; set; }
        public IEnumerable<FromAccountModel> ToPersonList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<PayMethod> PayMethodList { get; set; }
        public IEnumerable<BankTransferStatus> StatusList { get; set; }
        public IEnumerable<BankAccount> BankAccountList { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<CashActions> Result { get; set; }

        public VDocumentCashCollections CashDetail { get; set; }

    }
}