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
    }
}