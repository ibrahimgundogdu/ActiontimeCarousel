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
        public string ExchangeRate { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
    }

    public class NewCashOpen
    {
        public int ActinTypeID { get; set; }
        public int LocationID { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
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
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
    }
    public class NewSalaryEarn
    {
        public int ActinTypeID { get; set; }
        public float UnitPrice { get; set; }
        public float QuantityHour { get; set; }
        public string TotalAmount { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public string ExchangeRate { get; set; }
        public Guid UID { get; set; }
    }
    public class NewEmployeeSalary
    {
        public int EmployeeID { get; set; }
        public int OurCompanyID { get; set; }
        public string DateStart { get; set; }
        public string Hourly { get; set; }
        public string HourlyExtend { get; set; }
        public string ExtendMultiplyRate { get; set; }
    }
    public class NewLocationCash
    {
        public int LocationID { get; set; }
        public string CashName { get; set; }
        public string BlockedAmount { get; set; }
        public string Currency { get; set; }
    }
    public class NewBank
    {
        public int OurCompanyID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
    public class NewBankAccount
    {
        public int BankID { get; set; }
        public int AccountTypeID { get; set; }
        public string BranchName { get; set; }
        public string AccountName { get; set; }
        public string BranchCode { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public string IBAN { get; set; }
        
    }
    public class NewPosTerminal
    {
        public int LocationID { get; set; }
        public int BankAccountID { get; set; }
        public string ClientID { get; set; }
        public string TerminalID { get; set; }
        public string BrandName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }

    }
    public class NewCashRecord
    {
        public int LocationID { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }

    }
}