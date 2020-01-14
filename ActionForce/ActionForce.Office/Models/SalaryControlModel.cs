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
        public IEnumerable<Cash> CashList { get; set; }
        public IEnumerable<Location> LocationList { get; set; }
        public IEnumerable<FromAccountModel> FromList { get; set; }
        public IEnumerable<Currency> CurrencyList { get; set; }
        public IEnumerable<BankAccount> BankAccountList { get; set; }
        public IEnumerable<VEmployeeSalary> UnitSalaryList { get; set; }
        public IEnumerable<VEmployeeSalaryDist> UnitSalaryDistList { get; set; }
        public VEmployeeSalary UnitSalary { get; set; }

        public FilterModel Filters { get; set; }
        public OurCompany CurrentCompany { get; set; }
        public VLocation CurrentLocation { get; set; }
        public Result<CashActions> Result { get; set; }
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

    }
}