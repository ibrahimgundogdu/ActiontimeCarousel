using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class LocationEmployee
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string FotoFile { get; set; }
        public Guid? Token { get; set; }
        public string Position { get; set; }
    }


    public class LocationRoleGroup
    {
        public int ID { get; set; }
        public string GroupName { get; set; }
        public int RoleLevel { get; set; }
    }

    public class LocationOurCompany
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Culture { get; set; }
        public int TimeZone { get; set; }
    }

    public class LocationInfo
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public Guid? UID { get; set; }
        public int TimeZone { get; set; }
    }
}