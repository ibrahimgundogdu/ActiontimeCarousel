using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class LocationPartModel
    {
        public int PartId { get; set; }
        public int SortNumber { get; set; }
        public string PartName { get; set; }
        public string Device { get; set; }
        public string Ticket { get; set; }
		public DateTime StartTime { get; set; }
        public DateTime NowTime { get; set; }
        public DateTime LocalTime { get; set; }
        public int Duration { get; set; }
        public int TimeElapsed { get; set; }
        public int TimeRemain { get; set; }
        public string EmployeeName { get; set; }
        public int Status { get; set; }


    }
}