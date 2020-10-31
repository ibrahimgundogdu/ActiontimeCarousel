using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class CashCollectionModel
    {
        public int ActionTypeID { get; set; }
        public string ActionTypeName { get; set; }
        public int? FromEmployeeID { get; set; }
        public int? FromBankAccountID { get; set; }
        public int? FromCustomerID { get; set; }
        public int LocationID { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? DocumentDate { get; set; }
        public string Description { get; set; }
        public int? EnvironmentID { get; set; }
        public long? ReferanceID { get; set; }
        public long? ResultID { get; set; }
        public bool IsActive { get; set; }
    }
}
