using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeeControlModel : LayoutControlModel
    {

        public Result Result { get; set; }
        public CheckEmployee CheckEmployee { get; set; }
        public IEnumerable<Employee> Employees { get; set; }
        public VEmployeeAll Employee { get; set; }
        public IEnumerable<Employee> AbsoluteEmployees { get; set; }
        public IEnumerable<Employee> OptionalEmployees { get; set; }



        public IEnumerable<VEmployeeAll> EmployeeList { get; set; }
        //public GetEmployeeAll_Result3 EmpList { get; set; }

        public EmployeeFilterModel FilterModel { get; set; }
        public WizardModel Wizard { get; set; }
        
        
        public List<ApplicationLog> LogList { get; set; }
        public List<VEmployeeLocation> EmployeeLocationList { get; set; }
        public List<VEmployeeLocationPosition> EmployeeLocationPositions { get; set; }
        public int empID { get; set; }

        public IEnumerable<IdentityType> IdentityTypes { get; set; }
        public IEnumerable<OurCompany> OurList { get; set; }
        public IEnumerable<CountryPhoneCode> PhoneCodes { get; set; }
        public IEnumerable<RoleGroup> RoleGroupList { get; set; }
        public IEnumerable<EmployeeAreaCategory> AreaCategoryList { get; set; }
        public IEnumerable<Department> DepartmentList { get; set; }
        public IEnumerable<EmployeePositions> PositionList { get; set; }
        public IEnumerable<EmployeeStatus> StatusList { get; set; }
        public IEnumerable<EmployeeShiftType> ShiftTypeList { get; set; }
        public IEnumerable<EmployeeSalaryCategory> SalaryCategoryList { get; set; }
        public IEnumerable<EmployeeSequence> SequenceList { get; set; }
        public IEnumerable<Bank> BankList { get; set; }
        public Bank Bank { get; set; }


        public IEnumerable<VEmployeeCashActions> CashAction { get; set; }
        public IEnumerable<VDocumentSalaryEarn> SalaryEarn { get; set; }
        public SetcardParameter SetcardParameter { get; set; }


        public IEnumerable<EmployeeShift> EmployeeShifts { get; set; }
        public IEnumerable<Schedule> EmployeeSchedules { get; set; }
        public IEnumerable<EmployeeShift> EmployeeBreaks { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public Location modelLokation { get; set; }
        public IEnumerable<EmployeeLocation> EmployeeLocations { get; set; }
        public IEnumerable<EmployeePeriods> EmployeePeriods { get; set; }
        public IEnumerable<EmployeeSalary> EmployeeSalary { get; set; }
        public IEnumerable<DocumentEmployeePermit> EmployeePermits { get; set; }
        public IEnumerable<VSystemCheckEmployeeRows> EmployeeCheck { get; set; }
        public IEnumerable<VSystemCheckEmployeePerformance> EmployeeCheckPerformans { get; set; }
        public IEnumerable<VPersonalDocument> PersonalDocument { get; set; }
        public IEnumerable<PersonalDocumentType> PersonalDocumentType { get; set; }
        public IEnumerable<PersonalDocument> PersonalDocumentList { get; set; }
        public DateList CurrentDate { get; set; }

        public List<DateList> WeekList { get; set; }

        public string WeekCode { get; set; }
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

        public VEmployeeLocation CurrentLocation { get; set; }

        public IEnumerable<VEmployeeCashActions> EmployeeActionList { get; set; }

        public IEnumerable<TotalModel> HeaderTotals { get; set; }
        public IEnumerable<TotalModel> MiddleTotals { get; set; }
        public IEnumerable<TotalModel> FooterTotals { get; set; }

        public IEnumerable<TotalFood> HeaderTotal { get; set; }
        public IEnumerable<TotalFood> MiddleTotal { get; set; }
        public IEnumerable<TotalFood> FooterTotal { get; set; }




        public IEnumerable<VEmployee> VEmployee { get; set; }

        
        public EmployeeShift EmployeeShift { get; set; }
        public EmployeeShift EmployeeBreak { get; set; }
        
        public Schedule EmployeeSchedule { get; set; }


        public Employee CurrentEmployee { get; set; }

        public string TodayDateCode { get; set; }
        public string CurrentDateCode { get; set; }
        public string NextDateCode { get; set; }
        public string PrevDateCode { get; set; }

        
        
        public IEnumerable<FromAccountModel> FromList { get; set; }
        
        
        public OurCompany CurrentCompany { get; set; }
        

        

        public IEnumerable<VLocationSchedule> VLocationSchedule { get; set; }
        public IEnumerable<VSchedule> EmpSchedule { get; set; }

        public IEnumerable<Country> CountryList { get; set; }
        public IEnumerable<State> StateList { get; set; }
        public IEnumerable<City> CityList { get; set; }

        public IEnumerable<VEmployeePhones> EmployeePhones { get; set; }
        public IEnumerable<PhoneType> PhoneTypes { get; set; }
        public IEnumerable<VEmployeeEmails> EmployeeEmails { get; set; }
        public IEnumerable<EmailType> EmailTypes { get; set; }
        public IEnumerable<VEmployeeAddress> EmployeeAddress { get; set; }
        public IEnumerable<AddressType> AddressTypes { get; set; }


        public VEmployeePhones EmployeePhone { get; set; }
        public VEmployeeEmails EmployeeEmail { get; set; }
        public VEmployeeAddress EmployeeAdres { get; set; }



    }

    public class EmployeeFilterModel
    {
        public string FullName { get; set; }
        public int IsActive { get; set; }
        public int? LocationID { get; set; }
        public int? EmployeeID { get; set; }
        public int? DepartmentID { get; set; }
        public int? PositionID { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public Guid? LocationUID { get; set; }
        public Guid? EmployeeUID { get; set; }
        public string SearchKey { get; set; }
        public int? AreaID { get; set; }
        
    }
    
}