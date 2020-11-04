using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class CashControlModel : LayoutControlModel
    {
        public DateTime SelectedDate { get; set; }
        public IEnumerable<Cash> Cashes { get; set; }
        public Cash CurrentCash { get; set; }
        public IEnumerable<Currency> Currencies { get; set; }
        public IEnumerable<DocumentCashCollections> CashCollections { get; set; }
        public DocumentCashCollections CashCollection { get; set; }
        public string EmployeeRecorded { get; set; }
        public string EmployeeUpdated { get; set; }
        public bool IsUpdatible { get; set; } = false;


        public IEnumerable<DocumentCashPayments> CashPayments { get; set; }
        public DocumentCashPayments CashPayment { get; set; }

        public SummaryControlModel Summary { get; set; }


        public IEnumerable<ExpenseType> ExpenseTypes { get; set; }
        public IEnumerable<DocumentCashExpense> CashExpenses { get; set; }
        public DocumentCashExpense CashExpense { get; set; }
        public DateTime? ReceiptDate { get; set; }


        public IEnumerable<DocumentSaleExchange> CashSaleExchanges { get; set; }
        public DocumentSaleExchange CashSaleExchange { get; set; }


        public IEnumerable<DocumentBuyExchange> CashBuyExchanges { get; set; }
        public DocumentBuyExchange CashBuyExchange { get; set; }
        public Exchange Exchange { get; set; }


        public IEnumerable<SalaryPayment> SalaryPayments { get; set; }
        public DocumentSalaryPayment SalaryPayment { get; set; }
        public IEnumerable<Employee> Employees { get; set; }


        public IEnumerable<BankTransfer> BankTransfers { get; set; }
        public DocumentBankTransfer BankTransfer { get; set; }
        public IEnumerable<VBankAccount> BankAccountList { get; set; }
        public IEnumerable<BankTransferStatus> TransferStatus { get; set; }
        
    }
}