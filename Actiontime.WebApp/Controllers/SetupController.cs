using Actiontime.Data.Context;
using Actiontime.DataCloud.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.WebApp.Controllers
{
	[Authorize]
	public class SetupController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ApplicationCloudDbContext _cdbContext;
		private readonly ILogger<HomeController> _logger;
		public SetupController(ApplicationDbContext databaseContext, ApplicationCloudDbContext cdbContext, ILogger<HomeController> logger)
		{
			_dbContext = databaseContext;
			_cdbContext = cdbContext;
			_logger = logger;
		}

		public IActionResult Index()
		{
			SetupControlModel model = new SetupControlModel();
			var currentDate = DateTime.Now.Date;

			model.Location = _dbContext.OurLocations.FirstOrDefault();
			if (model.Location != null)
			{
				currentDate = model.Location.LocalDate.Value;
			}
			model.Employees = _dbContext.Employees.ToList();
			model.EmployeeSchedules = _dbContext.EmployeeSchedules.Where(x => x.ScheduleDate == currentDate).ToList();
			model.LocationSchedules = _dbContext.LocationSchedules.Where(x => x.ScheduleDate == currentDate).ToList();
			model.EmployeeShifts = _dbContext.EmployeeShifts.Where(x => x.ShiftDate == currentDate).ToList();
			model.CLocations = _cdbContext.Locations.Where(x => x.OurCompanyId == 1 && x.IsActive == true).OrderBy(x => x.SortBy).ToList();
			model.LocationPartials = _dbContext.LocationPartials.ToList();
			model.ProductPrices = _dbContext.ProductPrices.ToList();
			model.CashActionTypes = _dbContext.CashActionTypes.ToList();


			return View(model);
		}

		[HttpPost]
		public IActionResult Location(LocationViewModel model)
		{

			if (ModelState.IsValid)
			{
				var sql = "EXEC GetLocations @p0";
				_dbContext.Database.ExecuteSqlRaw(sql, model.LocationId);
			}

			return RedirectToAction("Index");
		}


		public IActionResult Employee()
		{

			if (ModelState.IsValid)
			{
				var sql = "EXEC GetEmployees";
				_dbContext.Database.ExecuteSqlRaw(sql);
			}

			return RedirectToAction("Index");
		}
		public IActionResult LocationPartial(int id)
		{

			if (ModelState.IsValid)
			{
				var sql = "EXEC GetLocationParts @p0";
				_dbContext.Database.ExecuteSqlRaw(sql, id);
			}

			return RedirectToAction("Index");
		}
		public IActionResult Schedule()
		{

			if (ModelState.IsValid)
			{
				var sql = "EXEC GetLocationSchedules";
				_dbContext.Database.ExecuteSqlRaw(sql);

				var sql2 = "EXEC GetEmployeeSchedules";
				_dbContext.Database.ExecuteSqlRaw(sql2);

			}

			return RedirectToAction("Index");
		}

		public IActionResult Price(int id)
		{

			if (ModelState.IsValid)
			{
				var sql = "EXEC GetLocationProductPrices @p0";
				_dbContext.Database.ExecuteSqlRaw(sql, id);
			}

			return RedirectToAction("Index");
		}

		public IActionResult Lookup()
		{

			if (ModelState.IsValid)
			{
				var sql = "EXEC GetLookups";
				_dbContext.Database.ExecuteSqlRaw(sql);


				sql = "EXEC GetLookupsInspection";
				_dbContext.Database.ExecuteSqlRaw(sql);
			}

			return RedirectToAction("Index");
		}



	}
}
