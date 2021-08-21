using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class RefundFormModel
    {
        public long? ExpenseSlipID { get; set; } //
        public long OrderID { get; set; } //
        public string OrderNumber { get; set; }
        public string PaymentAmount { get; set; }
        public string DocumentNumber { get; set; } //
        public DateTime DocumentDate { get; set; } //

        public string CountryCode { get; set; }  //phone-number-country   phone-number   phone-number-full   phone-number-code
        public string CustomerPhone { get; set; }
        public string PhoneNumberFull { get; set; }
        public string PhoneNumberCountry { get; set; }
        public string PostAddress { get; set; }

        public string CustomerName { get; set; } 
        public string CustomerIdentityNumber { get; set; }
        public string CustomerMail { get; set; }
        public string Description { get; set; } //
    }
}