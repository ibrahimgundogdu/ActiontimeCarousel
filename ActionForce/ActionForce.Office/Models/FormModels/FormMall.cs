using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FormMall
    {
        public int ID { get; set; }
        public int OurCompanyID { get; set; }
        public string FullName { get; set; }
        public int MallSegmentID { get; set; }
        public int MyProperty { get; set; }
        public string StructuralCondition { get; set; }
        public string Address { get; set; }
        public int Country { get; set; }
        public int State { get; set; }
        public int City { get; set; }
        public int County { get; set; }
        public string PostCode { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Map { get; set; }
        public int InvestorCompanyID { get; set; }
        public int LeasingCompanyID { get; set; }
        public int IsLeasingInHouse { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string ContactPhoneCode { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public DateTime RecordDate { get; set; }
        public int RecordEmployeeID { get; set; }
        public string RecordIP { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateEmployeeID { get; set; }
        public string UpdateIP { get; set; }
        public Nullable<System.Guid> MallUID { get; set; }
        public Nullable<int> Timezone { get; set; }
        public bool IsActive { get; set; }
    }

    public class FormMallContact
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsMaster { get; set; }
        public bool IsActive { get; set; }
    }

    public class FormMallLocationContract
    {
        public int ID { get; set; }
        public int MallID { get; set; }
        public int LocationID { get; set; }
        public string LocationDescription { get; set; }
        public decimal RentAmount { get; set; }
        public string Currency { get; set; }
        public decimal CommonExpenseAmount { get; set; }
        public decimal GuaranteeAmount { get; set; }
        public decimal StampDutyAmount { get; set; }
        public string ContractDuration { get; set; }
        public DateTime ContractDateBegin { get; set; }
        public DateTime ContractDateEnd { get; set; }
        public string ContractFile { get; set; }
        public bool IsActive { get; set; }
    }
}