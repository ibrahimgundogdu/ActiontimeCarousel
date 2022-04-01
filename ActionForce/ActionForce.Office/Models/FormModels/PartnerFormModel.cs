using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnerFormModel
    {
        public int PartnerID { get; set; }
        public string AccountCode { get; set; }
        public string FullName { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
        public string EMail { get; set; }
        public string PhoneCode { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int? Country { get; set; }
        public int? StateID { get; set; }
        public int? CityID { get; set; }
        public string PostCode { get; set; }
        public string IsActive { get; set; }
        public Guid? UID { get; set; }
    }
}