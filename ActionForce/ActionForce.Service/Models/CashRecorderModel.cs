using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class CashRecorderModel
    {
        public int ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public int LocationID { get; set; }
        public double CashAmount { get; set; }
        public double CreditAmount { get; set; }
        public double NetAmount { get; set; }
        public double TotalAmount { get; set; }
        public string Currency { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ProcessDate { get; set; }
        public DateTime SlipDate { get; set; }
        public int? EnvironmentID { get; set; }
        public string SlipNumber { get; set; }
        public string SlipFile { get; set; }
        public long? ResultID { get; set; }
        public Guid? UID { get; set; }
        public string SlipPath { get; set; }
        public bool IsActive { get; set; }
    }
}
