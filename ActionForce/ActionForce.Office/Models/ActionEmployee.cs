using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ActionEmployee
    {
        public int EmployeeID { get; set; }
        public string Title { get; set; }
        public string FullName { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public string FotoFile { get; set; }
        public string Token { get; set; }
        public Nullable<int> OurCompanyID { get; set; }
        public Nullable<int> RoleGroupID { get; set; }
        public virtual OurCompany OurCompany { get; set; }
        public virtual ActionRoleGroup RoleGroup { get; set; }
    }

    public class ActionRoleGroup
    {
        public int ID { get; set; }
        public string GroupName { get; set; }
        public int RoleLevel { get; set; }

    }
}