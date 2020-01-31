using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class Employees
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public string IdentityType { get; set; }
        public string IdentityNumber { get; set; }
        public string Mobile2 { get; set; }
        public string Whatsapp { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FotoFile { get; set; }
        public int OurCompanyID { get; set; }
        public int LocationID { get; set; }
        public int ShiftTypeID { get; set; }
        public int StatusID { get; set; }
        public int RoleGroupID { get; set; }
        public int DepartmentID { get; set; }
        public int AreaCategoryID { get; set; }
        public int SalaryCategoryID { get; set; }
        public int SequenceID { get; set; }
        public int PositionID { get; set; }
        public string Description { get; set; }
        public bool? IsTemp { get; set; }
        public bool? IsActive { get; set; }
        public Guid? EmployeeUID { get; set; }
    }

    
}