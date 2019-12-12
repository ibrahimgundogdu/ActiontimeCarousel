using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class DefinitionControlModel : LayoutControlModel
    {
        public IEnumerable<VCash> CashList { get; set; }
        public VCash Cash { get; set; }

        public IEnumerable<Bank> BankList { get; set; }
        public Bank Bank { get; set; }

        public IEnumerable<VBankAccount> BankAccountList { get; set; }
        public VBankAccount BankAccount { get; set; }

        public IEnumerable<VCashRecorders> CashRecordList { get; set; }
        public VCashRecorders CashRedord { get; set; }

        public IEnumerable<CashRecorders> CashRecordsList { get; set; }

        public IEnumerable<VLocationPosTerminal> LocPosTerminalList { get; set; }
        public IEnumerable<VPosTerminal> PosTerminalList { get; set; }
        public VLocationPosTerminal LocPosTerminal { get; set; }

        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<BankAccountType> AccountType { get; set; }
        public IEnumerable<VBankAccount> AccountList { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<CashActions> Result { get; set; }
        public Result<BankActions> Results { get; set; }
        public IEnumerable<ApplicationLog> History { get; set; }

    }
}