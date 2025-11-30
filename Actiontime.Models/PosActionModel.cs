using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class PosActionModel
    {

        public string? SourceName { get; set; }

        public int? ActionTypeId { get; set; }

        public DateTime? ActionDate { get; set; }

        public string? ProcessName { get; set; }

        public string ProcessType { get; set; } = null!;

        public double? Amount { get; set; }

        public string? Currency { get; set; }

        public DateTime? RecordDate { get; set; }

        public string ProcessUid { get; set; }
    }
}
