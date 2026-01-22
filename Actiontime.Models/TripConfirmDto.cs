using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class TripConfirmDto
    {
        public long Id { get; set; }
        public string? ConfirmNumber { get; set; }
        public long SaleOrderId { get; set; }
        public long SaleOrderRowId { get; set; }
        public long TripRoundId { get; set; }
        public string ReaderSerialNumber { get; set; } = null!;
        public string? ConfirmTime { get; set; }
        public string TicketNumber { get; set; } = null!;
    }
}
