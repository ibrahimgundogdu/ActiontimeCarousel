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

        public List<LocationModel> LocationList { get; set; }
        public LocationModel LocationModel { get; set; }
        public List<string> StateList { get; set; }
        public List<string> TypeList { get; set; }
        public LocationFilterModel FilterModel { get; set; }
        public List<ApplicationLog> LogList { get; set; }
        public List<VEmployeeLocation> EmployeeLocationList { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
    }

    public class LocationFilterModel
    {
        public string LocationName { get; set; }
        public string State { get; set; }
        public string TypeName { get; set; }
        public int IsActive { get; set; }
    }

    public class LocationModel
    {
        public int LocationID { get; set; }
        public int OurCompany { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string LocationNameSearch { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Nullable<int> Timezone { get; set; }
        public string MapURL { get; set; }
        public Nullable<bool> IsHaveOperator { get; set; }
        public string IP { get; set; }
        public string SortBy { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string TypeName { get; set; }
        public string Currency { get; set; }
        public Nullable<System.Guid> LocationUID { get; set; }
    }
}