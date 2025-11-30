using Actiontime.Data.Context;
using Actiontime.DataCloud.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.WebApp.Controllers
{
	public class DeviceController : Controller
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly ApplicationCloudDbContext _cdbContext;
		private readonly ILogger<HomeController> _logger;
		public DeviceController(ApplicationDbContext databaseContext, ApplicationCloudDbContext cdbContext, ILogger<HomeController> logger)
		{
			_dbContext = databaseContext;
			_cdbContext = cdbContext;
			_logger = logger;
		}

		public IActionResult Index(string? id)
		{
			DeviceControlModel model = new DeviceControlModel();


			if (!string.IsNullOrEmpty(id))
			{
				model.QRReader = _dbContext.Qrreaders.FirstOrDefault(x => x.SerialNumber == id);
			}

			model.Location = _dbContext.OurLocations.FirstOrDefault();
			model.QRReaders = _dbContext.Qrreaders.Where(x=> x.LocationId == model.Location.Id).ToList();
			model.LocationPartials = _dbContext.LocationPartials.ToList();
			model.PartId = model.QRReader?.LocationPartId ?? 0;
			model.Items = model.LocationPartials.Select(x => new SelectListItem()
			{
				Value = x.Id.ToString(),
				Text = $"#{x.Id} {x.PartName}"
			}).ToList();

			return View(model);
		}


		//PartUpdate
		[HttpPost]
		public IActionResult PartUpdate(DeviceViewModel model)
		{

			if (ModelState.IsValid)
			{
                var location = _dbContext.OurLocations.FirstOrDefault();
                var device = _dbContext.Qrreaders.FirstOrDefault(x=> x.SerialNumber == model.SerialNumber);

				if (device != null)
				{
					device.PartName = model.PartName;
					device.LocationPartId = model.PartId;
					device.LocationId = location?.Id;

					_dbContext.SaveChanges();
				}

			}

			return RedirectToAction("Index", new {id=model.SerialNumber});
		}

	}
}
