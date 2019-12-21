using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ResultControlModel : LayoutControlModel
    {

        public VDayResult CurrentDayResult { get; set; }
        public DayResult DayResult { get; set; }
        public IEnumerable<VDayResult> DayResultList { get; set; }
        public IEnumerable<DayResultItems> DayResultItems { get; set; }
        public IEnumerable<VDayResultItemList> DayResultItemList { get; set; }
        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<VLocationSchedule> LocationScheduleList { get; set; }
        public IEnumerable<VLocationShift> LocationShiftList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<VBankAccount> BankAccountList { get; set; }
        public IEnumerable<DocumentType> DocumentTypes { get; set; }
        public IEnumerable<CashActionType> CashActionTypes { get; set; }
        public IEnumerable<BankActionType> BankActionTypes { get; set; }
        public IEnumerable<ExpenseType> ExpenseTypes { get; set; }

        public IEnumerable<VCashActions> CashActions { get; set; }
        public IEnumerable<VEmployeeCashActions> EmployeeActions { get; set; }
        public IEnumerable<VBankActions> BankActions { get; set; }
        public IEnumerable<VDocumentSaleExchange> Exchanges { get; set; }
        public IEnumerable<VDocumentCashExpense> Expenses { get; set; }
        public IEnumerable<VDocumentBankTransfer> BankTransfers { get; set; }
        public IEnumerable<DocumentCashRecorderSlip> CashRecorderSlips { get; set; }
        public IEnumerable<VDayResultDocuments> DayResultDocuments { get; set; }
        



        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public Location CurrentLocation { get; set; }
        public DateList CurrentDate { get; set; }
        public Result<DayResult> Result { get; set; }


        public string TodayDateCode { get; set; }
        public string CurrentDateCode { get; set; }
        public string NextDateCode { get; set; }
        public string PrevDateCode { get; set; }
    }
}