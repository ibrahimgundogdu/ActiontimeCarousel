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
        public LocationFilterModel FilterModel { get; set; }
        public List<ApplicationLog> LogList { get; set; }
        public List<LocationEmployeeModel> EmployeeLocationList { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
        public List<VPriceCategory> PriceCategoryList { get; set; }
        public List<City> CityList { get; set; }
        public List<Mall> MallList { get; set; }
        public List<VBankAccount> BankAccountList { get; set; }
        public List<LocationType> LocationTypeList { get; set; }
        public Nullable<int> RecordEmployeeID { get; set; }
        public Nullable<System.DateTime> RecordDate { get; set; }
        public Nullable<int> UpdateEmployeeID { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        #region Schedule
        public string ScheduleStart { get; set; }
        public string ScheduleFinish { get; set; }
        public string ScheduleTime { get; set; }
        #endregion
        #region Shift
        public string ShiftStart { get; set; }
        public string ShiftFinish { get; set; }
        public string ShiftTime { get; set; }
        #endregion
        #region Status
        public string StatusName { get; set; }
        public string StatusClass { get; set; }
        public string StatusIcon { get; set; }
        #endregion
        #region ScheduleLocation
        public List<VLocationSchedule> LocationScheduleList { get; set; }
        public string WeekCode { get; set; }
        #endregion
        #region ShiftLocation
        public List<DateList> WeekList { get; set; }
        public List<VLocationShift> LocationShiftList { get; set; }
        #endregion
    }
}