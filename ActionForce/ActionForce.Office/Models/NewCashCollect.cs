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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
    }

    public class EditCashCollect
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
        public string IsActive { get; set; }
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public int PayMethodID { get; set; }
        public Guid UID { get; set; }
    }
    public class EditCashSale
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public int PayMethodID { get; set; }
        public Guid UID { get; set; }
        public string IsActive { get; set; }
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
        public string SaleExchangeRate { get; set; }
        public long? ReferanceID { get; set; }
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public string DocumentDate { get; set; }
        public int? docDate { get; set; }
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
    }
    public class EditCashPayments
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public string IsActive { get; set; }
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
    }
    public class EditTicketSaleReturn
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
        public string IsActive { get; set; }
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
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public int? ExpenseTypeID { get; set; }
        public string ReferanceModel { get; set; }
        public string SlipDate { get; set; }
        public string SlipTime { get; set; }
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
        public string SlipDate { get; set; }
        public string SlipTime { get; set; }
        public int StatusID { get; set; }
        public string ReferenceCode { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public string TrackingNumber { get; set; }
        public string Commission { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
        public string IsActive { get; set; }
        public string ReferanceModel { get; set; }
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
        public long? ReferanceID { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public int? CategoryID { get; set; }
        public Guid UID { get; set; }
    }

    public class EditCashSalaryPayment
    {
        public Guid UID { get; set; }
        public long ID { get; set; }
        public string DocumentDate { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public int SalaryTypeID { get; set; }
        public int CategoryID { get; set; }
        public int ActionTypeID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ExchangeRate { get; set; }
        public string Description { get; set; }
        public string IsActive { get; set; }
        public int FromBankID { get; set; }
    }

    public class CashSalaryPayment
    {
        public int EmployeeID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int SalaryTypeID { get; set; }
        public int CategoryID { get; set; }
        public int FromBankID { get; set; }
    }


    public class NewPosCollect
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int BankAccountID { get; set; }
        public string TerminalID { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
    }
    public class NewPosCancel
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int BankAccountID { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
    }
    public class NewPosReturn
    {
        public int ActinTypeID { get; set; }
        public string FromID { get; set; }
        public int LocationID { get; set; }
        public string Amount { get; set; }
        public int Quantity { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public int BankAccountID { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public Guid UID { get; set; }
    }
    public class NewSalaryEarn
    {
        public int ActinTypeID { get; set; }
        public float UnitPrice { get; set; }
        public float QuantityHour { get; set; }
        public string TotalAmount { get; set; }
        public int LocationID { get; set; }
        public string EmployeeID { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string Description { get; set; }
        public string ExchangeRate { get; set; }
        public string Exchange { get; set; }
        public long? ReferanceID { get; set; }
        public int? CategoryID { get; set; }
        public Guid UID { get; set; }
        public long? ID { get; set; }
        public double? SystemUnitPrice { get; set; }
        public double? SystemQuantityHour { get; set; }
        public double? SystemTotalAmount { get; set; }
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
    public class NewCashRecorder
    {
        public int ActinTypeID { get; set; }
        public int LocationID { get; set; }
        public string CashAmount { get; set; }
        public string CardAmount { get; set; }
        public string NetAmount { get; set; }
        public string TotalAmount { get; set; }
        public string Currency { get; set; }
        public string DocumentDate { get; set; }
        public string SlipDate { get; set; }
        public string SlipTime { get; set; }
        public string SlipNumber { get; set; }
        public string SlipFile { get; set; }
        public Guid UID { get; set; }
    }

    public class NewPermit
    {
        public string Date { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public int PermitTypeID { get; set; }
        public string DateBegin { get; set; }
        public string DateBeginHour { get; set; }
        public string DateEnd { get; set; }
        public string DateEndHour { get; set; }
        public string ReturnWorkDate { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
        
    }

    public class EditPermit
    {
        public int ActionTypeID { get; set; }
        public string Date { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public int PermitTypeID { get; set; }
        public string DateBegin { get; set; }
        public string DateBeginHour { get; set; }
        public string DateEnd { get; set; }
        public string DateEndHour { get; set; }
        public string ReturnWorkDate { get; set; }
        public string Description { get; set; }
        public int StatusID { get; set; }
        public Guid UID { get; set; }
        public long ID { get; set; }
        public string IsActive { get; set; }

    }

    public class NewEmployee
    {
        public string FullName { get; set; }
        public string Title { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public string IdentityType { get; set; }
        public string IdentityNumber { get; set; }
        public string Mobile2 { get; set; }
        public string Whatsapp { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public HttpPostedFile FotoFile { get; set; }
        public int OurCompanyID { get; set; }
        public int LocationID { get; set; }
        public int ShiftTypeID { get; set; }
        public int StatusID { get; set; }
        public int RoleGroupID { get; set; }
        public int DepartmentID { get; set; }
        public int AreaCategoryID { get; set; }
        public int SalaryCategoryID { get; set; }
        public int SequenceID { get; set; }
        public int PositionID { get; set; }
        public string Description { get; set; }
        public string IsTemp { get; set; }
        public string IsActive { get; set; }
        public Guid? EmployeeUID { get; set; }
    }

    public class EditedEmployee
    {
        public string FullName { get; set; }
        public string Title { get; set; }
        public string EMail { get; set; }
        public string CountryPhoneCode { get; set; }
        public string Mobile { get; set; }
        public string IdentityType { get; set; }
        public string IdentityNumber { get; set; }
        public string Mobile2 { get; set; }
        public string Whatsapp { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public HttpPostedFile FotoFile { get; set; }
        public int EmployeeID { get; set; }
        public int OurCompanyID { get; set; }
        public int ShiftTypeID { get; set; }
        public int StatusID { get; set; }
        public int RoleGroupID { get; set; }
        public int DepartmentID { get; set; }
        public int AreaCategoryID { get; set; }
        public int SalaryCategoryID { get; set; }
        public int SequenceID { get; set; }
        public int PositionID { get; set; }
        public string Description { get; set; }
        public string IsActive { get; set; }
        public Guid? EmployeeUID { get; set; }


        public int? Country { get; set; }
        public int? State { get; set; }
        public int? City { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }

    }
    public class NewEmployeeLocation
    {
        public int EmployeeID { get; set; }
        public int LocationID { get; set; }
        public int PositionID { get; set; }
        public string IsMaster { get; set; }
        public string IsActive { get; set; }
    }
    public class NewPeriods
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public int OurCompanyID { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }
        public string FinalFinishDate { get; set; }
        public string Description { get; set; }
    }
    public class NewDocument
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public int DocumentTypeID { get; set; }
        public string DocumentPath { get; set; }
        public string DocumentFile { get; set; }
        public string DocumentDescription { get; set; }
    }
    public class NewSalary
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public string DateStart { get; set; }
        public string Hourly { get; set; }
        public string Monthly { get; set; }
        public string HourlyExtend { get; set; }
        public string ExtendMultiplyRate { get; set; }
    }
}