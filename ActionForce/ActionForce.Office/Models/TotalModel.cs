using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class TotalModel
    {
        public string Type { get; set; }
        public double Total { get; set; }
        public double CiroTotal { get; set; }
        public string Currency { get; set; }
    }

    public class ResultTotalModel
    {
        public int ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public double Total { get; set; }
        public string Currency { get; set; }
    }
}