using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class TripRoundDto
    {

        public long Id { get; set; }

        public string RoundNumber { get; set; } = null!;

        public long RoundNumberInt { get; set; }

        public string RoundDate { get; set; }

        public string? RoundStart { get; set; }

        public string? RoundCancel { get; set; }

        public string? RoundEnd { get; set; }

        public string? TripDuration { get; set; }

        public string RecordDate { get; set; }

        public string Uid { get; set; }
        public int Count { get; set; } = 0;


    }
}
