using Actiontime.Data.Entities;

namespace Actiontime.WebApp
{
	public class SetupControlModel
	{
		public List<Employee> Employees { get; set; }
		public List<EmployeeSchedule> EmployeeSchedules { get; set; }
		public List<EmployeeShift> EmployeeShifts { get; set; }
		public OurLocation Location { get; set; }
		public List<DataCloud.Entities.Location> CLocations { get; set; }
		public LocationSchedule LocationSchedule { get; set; }
		public LocationShift LocationShift { get; set; }
		public List<LocationPartial> LocationPartials { get; set; }
		public List<LocationSchedule> LocationSchedules { get; set; }
		public List<ProductPrice> ProductPrices { get; set; }
		public List<CashActionType> CashActionTypes { get; set; }

	}
}
