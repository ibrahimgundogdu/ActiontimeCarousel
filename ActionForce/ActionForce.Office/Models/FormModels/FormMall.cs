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
        public string StructuralCondition { get; set; }
        public string Address { get; set; }
        public int CountryID { get; set; }
        public int StateID { get; set; }
        public int CityID { get; set; }
        public int CountyID { get; set; }
        public string PostCode { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Map { get; set; }
        public int InvestorCompanyID { get; set; }
        public int LeasingCompanyID { get; set; }
        public bool IsLeasingInHouse { get; set; }
        public DateTime RecordDate { get; set; }
        public int RecordEmployeeID { get; set; }
        public string RecordIP { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateEmployeeID { get; set; }
        public string UpdateIP { get; set; }
        public Nullable<System.Guid> MallUID { get; set; }
        public int Timezone { get; set; }
        public string IsActive { get; set; }


        public int MallContactID { get; set; }
        public int MallID { get; set; }
        public string Title { get; set; }
        public string ContactFullName { get; set; }
        public string PhoneCode { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsMaster { get; set; }
        public string MallContactIsActive { get; set; }


        public int MallContractID { get; set; }
        public int LocationID { get; set; }
        public string LocationDescription { get; set; }
        public string RentAmount { get; set; }
        public string Currency { get; set; }
        public string CommonExpenseAmount { get; set; }
        public string GuaranteeAmount { get; set; }
        public string StampDutyAmount { get; set; }
        public Nullable<System.DateTime> ContractDateBegin { get; set; }
        public Nullable<System.DateTime> ContractDateEnd { get; set; }
        public string ContractFile { get; set; }
        public string MallContractIsActive { get; set; }
    }
}