using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class SalaryControlModel : LayoutControlModel
    {
        public IEnumerable<VDocumentSalaryEarn> SalaryEarn { get; set; }
        public IEnumerable<VDocumentSalaryPayment> SalaryPayment { get; set; }
        public IEnumerable<VDocumentEmployeePermit> Permits { get; set; }
        public IEnumerable<EmployeeSalary> UnitPrice { get; set; }
        public Employee CurrentEmployee { get; set; }
        public IEnumerable<VEmployeeCashActions> EmployeeActionList { get; set; }
        public IEnumerable<Employee> EmployeeList { get; set; }
        public IEnumerable<PermitType> PermitTypes { get; set; }
        public IEnumerable<PermitStatus> PermitStatus { get; set; }

        public IEnumerable<SalaryCategory> SalaryCategories { get; set; }
        public IEnumerable<CashActionType> CashActionTypes { get; set; }
        public IEnumerable<SalaryType> SalaryTypes { get; set; }
        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<VBankAccount> BankAccountList { get; set; }
        public IEnumerable<VEmployeeSalary> UnitSalaryList { get; set; }
        public IEnumerable<VEmployeeSalaryDist> UnitSalaryDistList { get; set; }
        public VEmployeeSalary UnitSalary { get; set; }

        public IEnumerable<VSchedule> ScheduleList { get; set; }
        public IEnumerable<VEmpShift> ShiftList { get; set; }
        public IEnumerable<DateList> DateList { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result Result { get; set; }
        public Result InfoResult { get; set; }
        public IEnumerable<ApplicationLog> LogList { get; set; }

        public VDocumentSalaryEarn Detail { get; set; }
        public VDocumentSalaryPayment SalaryDetail { get; set; }
        public IEnumerable<ApplicationLog> History { get; set; }
        public EmployeeSalary EmployeeHour { get; set; }
        public VDocumentEmployeePermit CurrentPermit { get; set; }

        public IEnumerable<TotalModel> HeaderTotals { get; set; }
        public IEnumerable<TotalModel> MiddleTotals { get; set; }
        public IEnumerable<TotalModel> FooterTotals { get; set; }
        public IEnumerable<ResultTotalModel> ResultFooterTotals { get; set; }

        public List<EmployeeModel> EmployeeModels { get; set; }

        public VSalaryPeriod SalaryPeriod { get; set; }
        public List<VSalaryPeriod> SalaryPeriods { get; set; }
        public List<SalaryPeriodCompute> SalaryPeriodComputes { get; set; }

        public VSalaryPeriodComputeSum SalaryPeriodComputeSum { get; set; }

        public List<SalaryPeriodGroup> SalaryPeriodGroups { get; set; }
        public List<SalaryPeriodStatus> SalaryPeriodStatus { get; set; }
        public SalaryPeriodStatus SalaryPeriodNextStatus { get; set; }
        public List<int> Years { get; set; }

















    }
}