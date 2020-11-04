using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class BankTransferModel
    {
        public int ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public double Amount { get; set; }
        public double Commission { get; set; }
        public int LocationID { get; set; }
        public int? ToBankID { get; set; }
        public string Currency { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ProcessDate { get; set; }
        public string Description { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public int? EnvironmentID { get; set; }
        public Guid UID { get; set; }
        public string SlipNumber { get; set; }
        public string SlipDocument { get; set; }
        public DateTime? SlipDate { get; set; }
        public string SlipPath { get; set; }
        public int? StatusID { get; set; }
        public string ReferanceCode { get; set; }
        public string TrackingNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}
