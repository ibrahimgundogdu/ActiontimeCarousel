using Actiontime.Data.Context;
using Actiontime.DataCloud.Context;
using Actiontime.Services;
using Actiontime.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Actiontime.WebApp.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ApplicationCloudDbContext _cdbContext;
		private readonly ILogger<HomeController> _logger;
		public HomeController(ApplicationDbContext databaseContext, ApplicationCloudDbContext cdbContext, ILogger<HomeController> logger)
		{
			_dbContext = databaseContext;
			_cdbContext = cdbContext;
			_logger = logger;
		}

		public IActionResult Index()
		{
			HomeControlModel model = new HomeControlModel();
			DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);

			model.Location = _dbContext.OurLocations.FirstOrDefault();
			if (model.Location != null)
			{
				currentDate = model.Location.LocalDate.Value;
			}
			model.Employees = _dbContext.Employees.ToList();
			model.EmployeeSchedules = _dbContext.EmployeeSchedules.Where(x => x.ScheduleDate == currentDate).ToList();
			model.LocationSchedule = _dbContext.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == currentDate);
			model.EmployeeShifts = _dbContext.EmployeeShifts.Where(x => x.ShiftDate == currentDate).ToList();
			model.LocationShift = _dbContext.LocationShifts.FirstOrDefault(x => x.ShiftDate == currentDate);

			return View(model);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}