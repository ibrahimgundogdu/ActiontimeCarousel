using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class Payment
    {
        public int PaymentType { get; set; }
        public string PaymentSubType { get; set; }
        public string NumberOfInstallment { get; set; }
        public int PaymentAmount { get; set; }
        public string PaymentDesc { get; set; }
        public int PaymentCurrency { get; set; }
        public string PaymentInfo { get; set; }
        public string PaymentDateTime { get; set; }
        public short BankBKMID { get; set; }
        public List<SubPayment> subPayment { get; set; }
        public string BatchNumber { get; set; }
        public string StanNumber { get; set; }
        public string MerchantID { get; set; }
        public string TerminalID { get; set; }
        public string ReferenceNumber { get; set; }
        public string AuthorizationCode { get; set; }
        public string MaskedPan { get; set; }
    }
}