using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class FilterModel
    {
        public int? LocationID { get; set; }
        public int? EmployeeID { get; set; }
        public int? SalaryPeriodID { get; set; }
        public int? BankAccountID { get; set; }
        public int? TypeID { get; set; }
        public int? DepartmentID { get; set; }
        public int? PositionID { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public string IsActive { get; set; }
        public string SearchKey { get; set; }
        public int? StatusID { get; set; }
        public int? GroupID { get; set; }
        public int? Year { get; set; }

    }

    public class ResultFilterModel
    {
        public int? LocationID { get; set; }
        public int? BankAccountID { get; set; }
        public DateTime? ResultDate { get; set; }
        public long? ResultID { get; set; }
    }

    public class ExpenseFilterModel
    {
        public int? ExpenseCenterID { get; set; }
        public int? ExpenseItemID { get; set; }
        public int? ExpenseGroupID { get; set; }
        public int? DistributeGroupID { get; set; }
        public int? ExpenseStatusID { get; set; }
        public string ExpensePeriodCode { get; set; }
        public DateTime? DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public bool FromSearch { get; set; } = false;
    }
}