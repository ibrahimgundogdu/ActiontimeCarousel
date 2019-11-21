using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Integration.UfeService
{
    public class LocationShiftResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public LocationShiftData Data { get; set; }
    }

    public class LocationShiftData
    {
        public int ID { get; set; }
        public int LocationID { get; set; }
        public int? EmployeeID { get; set; }
        public int? EmployeeIDFinish { get; set; }
        public DateTime? ShiftDate { get; set; }
        public string ShiftStart { get; set; }
        public string ShiftFinish { get; set; }
        public DateTime? ShiftDateStart { get; set; }
        public DateTime? ShiftDateFinish { get; set; }
        public string Duration { get; set; }
        public int? DurationMinute { get; set; }
        public string LatitudeStart { get; set; }
        public string LongitudeStart { get; set; }
        public bool? FromMobileStart { get; set; }
        public string LatitudeFinish { get; set; }
        public string LongitudeFinish { get; set; }
        public bool? FromMobileFinish { get; set; }
        public int? RecordEmployeeID { get; set; }
        public DateTime? RecordDate { get; set; }
        public int? UpdateEmployeeID { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? EnvironmentID { get; set; }
        public int? CloseEnvironmentID { get; set; }
    }

    public class EmployeeShiftResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public EmployeeShiftData Data { get; set; }
    }


    public class EmployeeShiftData
    {
        public int ID { get; set; }
        public int EmployeeID { get; set; }
        public int LocationID { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftStart { get; set; }
        public string ShiftEnd { get; set; }
        public string Duration { get; set; }
        public bool? IsWorkTime { get; set; }
        public bool? IsBreakTime { get; set; }
        public DateTime? ShiftDateStart { get; set; }
        public DateTime? ShiftDateEnd { get; set; }
        public string ShiftDuration { get; set; }
        public int? DurationMinute { get; set; }
        public string BreakStart { get; set; }
        public string BreakEnd { get; set; }
        public string BreakDuration { get; set; }
        public string LatitudeStart { get; set; }
        public string LongitudeStart { get; set; }
        public bool? FromMobileStart { get; set; }
        public string LatitudeFinish { get; set; }
        public string LongitudeFinish { get; set; }
        public bool? FromMobileFinish { get; set; }
        public int? RecordEmployeeID { get; set; }
        public DateTime? RecordDate { get; set; }
        public int? UpdateEmployeeID { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string BreakTypeID { get; set; }
        public DateTime? BreakDateStart { get; set; }
        public DateTime? BreakDateEnd { get; set; }
        public int? BreakDurationMinute { get; set; }
        public int? EnvironmentID { get; set; }
        public int? CloseEnvironmentID { get; set; }
    }

}
