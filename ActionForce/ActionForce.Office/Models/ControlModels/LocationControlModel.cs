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
        public List<Tax> TaxList { get; set; }
        public List<LocationEmployeeModel> EmployeeLocationList { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
        public List<VPriceCategory> PriceCategoryList { get; set; }
        public List<VCity> CityList { get; set; }
        public List<Mall> MallList { get; set; }
        public List<VBankAccount> BankAccountList { get; set; }
        public List<LocationType> LocationTypeList { get; set; }
        public List<VLocationPosTerminal> LocationPosTerminalList { get; set; }
        public List<VPrice> LocationPriceLastList { get; set; }
        public List<VLocationPriceCategory> LocationPriceCategoryList { get; set; }
        public VLocationPriceCategory LocationPriceCategory { get; set; }
        public Nullable<int> RecordEmployeeID { get; set; }
        public Nullable<System.DateTime> RecordDate { get; set; }
        public Nullable<int> UpdateEmployeeID { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public int LocationPriceCategoryID { get; set; }
        public FormLocation CheckLocation { get; set; }
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
        public DateList CurrentDate { get; set; }
                
        public string NextWeekCode { get; set; }
        public string PrevWeekCode { get; set; }
        public string TodayWeekCode { get; set; }

        public string MoonCode { get; set; }
        public string NextMoonCode { get; set; }
        public string PrevMoonCode { get; set; }


        public DateList FirstWeekDay { get; set; }
        public DateList LastWeekDay { get; set; }

        public DateList FirstMoonDay { get; set; }
        public DateList LastMoonDay { get; set; }
        #endregion
        #region ShiftLocation
        public List<DateList> WeekList { get; set; }
        public List<VLocationShift> LocationShiftList { get; set; }
        #endregion
    }
}