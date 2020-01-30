using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class WizardModel
    {
        public string Identity { get; set; }
        public string IdentityNumber { get; set; }
        public string FullName { get; set; }
        public string EMail { get; set; }
        public string Mobile { get; set; }
        public int? EmployeeID { get; set; }
        public Guid? UID { get; set; }

        public List<string> Identitys { get; set; }
        public List<string> IdentityNumbers { get; set; }
        public List<string> FullNames { get; set; }
        public List<string> EMails { get; set; }
        public List<string> Mobiles { get; set; }
    }
}