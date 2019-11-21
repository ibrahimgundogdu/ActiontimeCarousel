using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class NewCashCollect
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public Guid UID { get; set; }
    }

    public class NewCashSale
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public int Quantity { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
    }
    public class NewCashExchange
    {
        public int ActinTypeID { get; set; }
        public int LocationID { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string Exchange { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
    }

    public class NewCashOpen
    {
        public int ActinTypeID { get; set; }
        public int LocationID { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
    }


    public class NewCashPayments
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
    }
    public class NewTicketSaleReturn
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public int Quantity { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int PayMethodID { get; set; }
    }

    public class NewCashExpense
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public string SlipNumber { get; set; }
        public string SlipDocument { get; set; }
    }
    public class NewCashBankTransfer
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public string SlipNumber { get; set; }
        public string SlipDocument { get; set; }
        public int StatusID { get; set; }
        public string ReferenceCode { get; set; }
    }
    public class NewCashSalaryPayment
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int CashID { get; set; }
        public int BankAccountID { get; set; }
        public int SalaryType { get; set; }
    }
    public class NewPosCollect
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int BankAccountID { get; set; }
        public string TerminalID { get; set; }
    }
    public class NewPosCancel
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int BankAccountID { get; set; }
    }
    public class NewPosReturn
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int BankAccountID { get; set; }
    }
}