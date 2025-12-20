using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class VOrderInfo
    {
        public int Id { get; set; }

        public string? SaleStatusName { get; set; }

        public string OrderNumber { get; set; } = null!;

        public DateOnly Date { get; set; }

        public int? LocationId { get; set; }

        public int? EmployeeId { get; set; }

        public double? TotalAmount { get; set; }

        public string? Sign { get; set; }

        public Guid Uid { get; set; }

        public DateTime? RecordDate { get; set; }

        public string? ReceiptNumber { get; set; }

        public int? PrintCount { get; set; }

        public int? PaymentType { get; set; }

        public double? PaymentAmount { get; set; }

        public string? PayMethodName { get; set; }

        public string? EmployeeFullName { get; set; }

        public int? TicketCount { get; set; }
        public List<VorderRow>? OrderRows { get; set; }
    }
}
