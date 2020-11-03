using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class SalaryPaymentModel
    {
        public int ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public int LocationID { get; set; }
        public int EmployeeID { get; set; }
        public int? FromCashID { get; set; }
        public int? FromBankID { get; set; }
        public int? CategoryID { get; set; }
        public int SalaryTypeID { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime ProcessDate { get; set; }
        public string Description { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public int? EnvironmentID { get; set; }
        public Guid UID { get; set; }
        public bool IsActive { get; set; }
    }
}
