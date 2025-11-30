using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class PersonInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? PhotoName { get; set; }
        public string StatusName { get; set; }
        public string UID { get; set; }
        public string DateSelected { get; set; }
        public string WeekSelected { get; set; }
        public string? ScheduleTime { get; set; }
        public string? ScheduleDuration { get; set; }
        public string? ShiftTime { get; set;}
        public string? ShiftDuration { get; set; }
        public string? BreakTotalTime { get; set; }
        public int BreakCount { get; set; }


    }
}
