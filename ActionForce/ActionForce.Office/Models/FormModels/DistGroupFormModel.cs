using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class DistGroupFormModel
    {
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public string SortBy { get; set; }
        public short GroupID { get; set; }
        public Guid? GroupUID { get; set; }
    }
}