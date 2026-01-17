using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class TicketCheck
    {
        public int Id { get; set; } = default!;
        public string QrCode { get; set; } = default!;
        public string LocationId { get; set; } = default!;
        public string LocationName { get; set; } = default!;
        public string TicketNumber { get; set; } = default!;
        public string CustomerName { get; set; } = "Guest";
        public DateTime? PurchaseDate { get; set; } = default!;
        public Guid? ConfirmNumber { get; set; }
        public DateTime? ConfirmTime { get; set; }
        public Guid? RoundUid { get; set; }
        public DateOnly? RoundDate { get; set; } = default!;
        public string RoundNumber { get; set; } = default!;
        public DateTimeOffset? RoundStart { get; set; }
        public DateTimeOffset? RoundEnd { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TicketStatus Status { get; set; } = TicketStatus.pending;
    }
}
