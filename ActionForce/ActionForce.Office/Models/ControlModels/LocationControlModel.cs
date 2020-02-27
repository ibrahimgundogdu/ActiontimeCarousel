using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ActionForce.Entity;

namespace ActionForce.Office
{
    public class LocationControlModel : LayoutControlModel
    {
        public Result Result { get; set; }
        public List<VLocation> LocationList { get; set; }
        public VLocation LocationModel { get; set; }
        public List<string> StateList { get; set; }
        //public List<string> TypeList { get; set; }
        public LocationFilterModel FilterModel { get; set; }
        public List<ApplicationLog> LogList { get; set; }
        public List<LocationEmployeeModel> EmployeeLocationList { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
        public List<PriceCategory> PriceCategoryList { get; set; }
        public List<Mall> MallList { get; set; }
        public List<VBankAccount> BankAccountList { get; set; }
        public List<LocationType> LocationTypeList { get; set; }
    }
}