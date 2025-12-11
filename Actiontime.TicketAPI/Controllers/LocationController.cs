using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class LocationController : ControllerBase
	{

		LocationService _locationService;
        CloudService _cloudService;

        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextFactory<ApplicationCloudDbContext> _cdbFactory;

        public LocationController(IDbContextFactory<ApplicationDbContext> dbFactory, IDbContextFactory<ApplicationCloudDbContext> cdbFactory, CloudService cloudService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _cdbFactory = cdbFactory ?? throw new ArgumentNullException(nameof(cdbFactory));
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
            _locationService = new LocationService(dbFactory, cdbFactory, _cloudService);
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
