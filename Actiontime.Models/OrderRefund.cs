using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class OrderRefund
    {
        public int Id { get; set; }

        public int SaleStatusId { get; set; }

        public string SaleStatusName { get; set; } = string.Empty;

        public string OrderNumber { get; set; } = null!;

        public DateTime Date { get; set; }

        public double TotalAmount { get; set; }

        public string Sign { get; set; } = string.Empty;

        public Guid Uid { get; set; }

        public DateTime RecordDate { get; set; }

        public int TicketCount { get; set; }

        public int PrintCount { get; set; }

        public double PaymentAmount { get; set; }

        public string PayMethodName { get; set; } = string.Empty;

        public double? RefundAmount { get; set; }

        public string? RefundMethodName { get; set; } = string.Empty;

        public string? RefundDescription { get; set; } = string.Empty;

        public string EmployeeFullName { get; set; } = string.Empty;

        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

    }
}
