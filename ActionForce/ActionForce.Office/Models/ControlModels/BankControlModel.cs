using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class BankControlModel : LayoutControlModel
    {
        public IEnumerable<VDocumentPosCollection> PosCollections { get; set; }
        public IEnumerable<VDocumentPosCancel> PosCancel { get; set; }
        public IEnumerable<VDocumentPosRefund> PosRefund { get; set; }

        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<BankAccount> BankAccountList { get; set; }

        public VDocumentPosCollection Detail { get; set; }
        public VDocumentPosCancel PosCancelDetail { get; set; }
        public VDocumentPosRefund PosRefundDetail { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<BankActions> Result { get; set; }
        public IEnumerable<ApplicationLog> History { get; set; }

        public List<VDocumentBankTransfer> DocumentBankTransfers { get; set; }
        public List<BankTransferStatus> BankTransferStatus { get; set; }
        public IEnumerable<VBankAccount> BankAccounts { get; set; }
        public DateTime SelectedDate { get; set; }
        public DateTime PrevDate { get; set; }
        public DateTime NextDate { get; set; }
        public List<BankTransferStatusCount> StatusCounts { get; set; }


    }
}