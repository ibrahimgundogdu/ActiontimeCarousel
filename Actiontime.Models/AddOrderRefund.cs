using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class AddOrderRefund
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int RefundTypeId { get; set; }
        public string Description { get; set; }
    }
}
