using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeControlModel : LayoutControlModel
    {
        public IEnumerable<VEmployeeList> EmployeeList { get; set; }
        public VEmployeeList EmpList { get; set; }


        public IEnumerable<OurCompany> OurList { get; set; }
        public IEnumerable<RoleGroup> RoleGroupList { get; set; }
        public IEnumerable<EmployeeAreaCategory> AreaCategoryList { get; set; }
        public IEnumerable<Department> DepartmentList { get; set; }
        public IEnumerable<EmployeePositions> PositionList { get; set; }
        public IEnumerable<EmployeeStatus> StatusList { get; set; }
        public IEnumerable<EmployeeShiftType> ShiftTypeList { get; set; }
        public IEnumerable<EmployeeSalaryCategory> SalaryCategoryList { get; set; }
        public IEnumerable<EmployeeSequence> SequenceList { get; set; }

        public Result Result { get; set; }

        public FilterModel Filters { get; set; }
        public IEnumerable<ApplicationLog> LogList { get; set; }



        public IEnumerable<VEmployee> VEmployee { get; set; }
        public IEnumerable<EmployeeLocation> EmployeeLocations { get; set; }

        public IEnumerable<EmployeeShift> EmployeeShifts { get; set; }
        public EmployeeShift EmployeeShift { get; set; }
        public IEnumerable<EmployeeShift> EmployeeBreaks { get; set; }
        public EmployeeShift EmployeeBreak { get; set; }
        public IEnumerable<Schedule> EmployeeSchedules { get; set; }
        public Schedule EmployeeSchedule { get; set; }

        public Employee CurrentEmployee { get; set; }


        public string TodayDateCode { get; set; }
        public string CurrentDateCode { get; set; }
        public string NextDateCode { get; set; }
        public string PrevDateCode { get; set; }

        
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        
        
        public OurCompany CurrentCompany { get; set; }
        public VEmployeeLocation CurrentLocation { get; set; }

        public DateList CurrentDate { get; set; }

        public IEnumerable<VLocationSchedule> VLocationSchedule { get; set; }
        public IEnumerable<VSchedule> EmpSchedule { get; set; }
        public IEnumerable<DateList> WeekList { get; set; }
        public DateList FirstWeekDay { get; set; }
        public DateList LastWeekDay { get; set; }

        public string WeekCode { get; set; }
        public string NextWeekCode { get; set; }
        public string PrevWeekCode { get; set; }

    }
}