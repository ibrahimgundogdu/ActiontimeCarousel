using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class NewPhone
    {
        public int EmployeeID { get; set; }
        public string CountryPhoneCode { get; set; }
        public string Mobile { get; set; }
        public int? PhoneType { get; set; }
        public string Description { get; set; }
        public string IsMaster { get; set; }
        public string IsActive { get; set; }

    }
    public class EditPhone
    {
        public int EEmployeeID { get; set; }
        public string ECountryPhoneCode { get; set; }
        public string EMobile { get; set; }
        public int? EPhoneType { get; set; }
        public string EDescription { get; set; }
        public string EIsMaster { get; set; }
        public string EIsActive { get; set; }
        public int? ID { get; set; }

    }

    public class NewEmail
    {
        public int EmployeeID { get; set; }
        public string EMail { get; set; }
        public int? EmailType { get; set; }
        public string Description { get; set; }
        public string IsMaster { get; set; }
        public string IsActive { get; set; }

    }

    public class EditEmail
    {
        public int EEmployeeID { get; set; }
        public string EEMail { get; set; }
        public int? EEmailType { get; set; }
        public string EDescription { get; set; }
        public string EIsMaster { get; set; }
        public string EIsActive { get; set; }
        public int? ID { get; set; }

    }

    public class NewAddress
    {
        public int EmployeeID { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public int? AddressType { get; set; }
        public int? Country { get; set; }
        public int? State { get; set; }
        public int? City { get; set; }
        public string Description { get; set; }
        public string IsMaster { get; set; }
        public string IsActive { get; set; }

    }

    public class EditAddress
    {
        public int EEmployeeID { get; set; }
        public string EAddress { get; set; }
        public string EPostCode { get; set; }
        public int? EAddressType { get; set; }
        public int? ECountry { get; set; }
        public int? EState { get; set; }
        public int? ECity { get; set; }
        public string EDescription { get; set; }
        public string EIsMaster { get; set; }
        public string EIsActive { get; set; }
        public int? ID { get; set; }

    }
}