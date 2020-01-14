using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class EmployeePermit
    {
        public int ActinTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public int OurCompanyID { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public int EnvironmentID { get; set; }
        public int TimeZone { get; set; }
        public int StatusID { get; set; }
        public Guid UID { get; set; }
        public DateTime Date { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public int PermitTypeID { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime ReturnWorkDate { get; set; }
        public string Description { get; set; }

        // for edit
        public long ID { get; set; }
        public bool IsActive { get; set; }
    }

    
}