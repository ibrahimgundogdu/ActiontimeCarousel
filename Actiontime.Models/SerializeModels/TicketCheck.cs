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
        public string Id { get; set; } = default!;
        public string LocationId { get; set; } = default!;
        public string LocationName { get; set; } = default!;
        public string QrCode { get; set; } = default!;
        public string TicketNumber { get; set; } = default!;
        public string CustomerName { get; set; } = "Guest";
        public string PurchaseDate { get; set; } = default!;
        public Guid? ConfirmUid { get; set; }
        public Guid? RoundUid { get; set; }
        public Guid? TripUid { get; set; }
        public string RoundNumber { get; set; } = default!;
        public DateTime? RoundStart { get; set; }
        public DateTime? RoundEnd { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TicketStatus Status { get; set; } = TicketStatus.pending;
    }
}
