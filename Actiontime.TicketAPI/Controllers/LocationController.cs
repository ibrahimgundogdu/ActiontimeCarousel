using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Actiontime.TicketAPI.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class LocationController : ControllerBase
	{

		LocationService _locationService;
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        public LocationController(ApplicationDbContext db, ApplicationCloudDbContext cdb)
		{
			_locationService = new LocationService(db, cdb);
		}

		[HttpGet()]
		public OurLocation? GetOurLocation()
		{
			return _locationService.GetOurLocation();
		}

		[HttpGet()]
		public OurLocationInfo GetLocationInfo()
		{
			return _locationService.GetOurLocationInfo();
		}

		[HttpGet()]
		public string CheckLocationShift(int employeeId)
		{
			return _locationService.CheckLocationShift(employeeId);
		}


		[HttpGet()]
		public List<ProductPriceModel>? GetPrices()
		{
			return _locationService.GetProductPrice();
		}


		[HttpGet()]
		public LocationSchedule? GetLocationSchedule(string Date)
		{
			var dateKey = DateTime.Now;

			DateTime.TryParse(Date, out dateKey);

			return _locationService.GetLocationSchedule(dateKey.Date);
		}

		[HttpGet()]
		public List<LocationSchedule>? GetLocationSchedules(string Date)
		{
			var dateKey = DateTime.Now;

			DateTime.TryParse(Date, out dateKey);

			return _locationService.GetLocationSchedules(dateKey.Date);
		}

		[HttpGet()]
		public List<LocationPartModel>? GetLiveParts()
		{
			return _locationService.GetLiveParts();
		}

		[HttpGet()]
		public void GetSync()
		{
			_locationService.GetSync();
		}


		[HttpGet()]
		public InspectionModel GetInspection(int empId)
		{
			return _locationService.GetInspection(empId);
		}

		[HttpGet()]
		public InspectionPartModel GetPartInspection(int inspectionId, int partId, int pageId, int empId)
		{
			return _locationService.GetPartInspection(inspectionId, partId, pageId, empId);
		}

		//SavePartInspection
		[HttpGet()]
		public AppResult SavePartInspection(int inspectionId, int partId, int pageId, int empId, string answer, string? description)
		{
			return _locationService.SavePartInspection(inspectionId, partId, pageId, empId, answer, description);
		}

		[HttpGet()]
		public AppResult CloseInspection(int inspectionId, int empId, string? description)
		{
			return _locationService.CloseInspection(inspectionId, empId, description);
		}

		[HttpGet()]
		public bool CheckInspection()
		{
			return _locationService.CheckInspection();
		}

		[HttpGet()]
		public string CompletePartRide(int partId, int employeeId)
		{
			return _locationService.CompletePartRide(partId, employeeId);
		}





	}
}
