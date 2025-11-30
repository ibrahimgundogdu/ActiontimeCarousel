using Actiontime.Data.Entities;

namespace Actiontime.WebApp
{
    public class HomeControlModel
    {
        public List<Employee> Employees { get; set; }
        public List<EmployeeSchedule> EmployeeSchedules { get; set; }
        public List<EmployeeShift> EmployeeShifts { get; set; }
		public OurLocation Location { get; set; }
		public LocationSchedule LocationSchedule { get; set; }
		public LocationShift LocationShift { get; set; }
	}
}
