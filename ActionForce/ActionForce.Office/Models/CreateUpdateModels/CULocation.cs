using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class CULocation
    {
        public int LocationID { get; set; }
        public int OurCompany { get; set; }
        public string LocationCode { get; set; }
        public string LocationName { get; set; }
        public string LocationNameSearch { get; set; }
        public string Description { get; set; }
        public int LocationTypeID { get; set; }
        public string State { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Nullable<int> Timezone { get; set; }
        public string MapURL { get; set; }
        public string IsHaveOperator { get; set; }
        public string IP { get; set; }
        public string SortBy { get; set; }
        public string IsActive { get; set; }
        public string TypeName { get; set; }
        public string Currency { get; set; }
        public string EnforcedWarning { get; set; }
        public Nullable<System.Guid> LocationUID { get; set; }
        public int PriceCatID { get; set; }
        public int MallID { get; set; }
        public int POSAccountID { get; set; }
        public int CityID { get; set; }
    }
}