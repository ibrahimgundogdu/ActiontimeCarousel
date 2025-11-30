using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class LocationInfo
    {

        public int Id { get; set; }
        public string LocationName { get; set; }
        public int TimeZone { get; set; }
        public DateTime LocalDate { get; set; }
        public DateTime LocalDateTime { get; set; }
        public double? TaxRate { get; set; }
        public short? PriceCatId { get; set; }
        public string? Currency { get; set; }
        public Guid? LocationUid { get; set; }
        public string? SortBy { get; set; }

    }
}
