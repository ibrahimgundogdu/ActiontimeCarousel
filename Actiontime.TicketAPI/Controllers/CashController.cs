using Actiontime.Data.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Microsoft.AspNetCore.Mvc;

namespace Actiontime.TicketAPI.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class CashController : ControllerBase
	{
		CashService _cashService;
		private IWebHostEnvironment _env;

		public CashController(IWebHostEnvironment env)
		{
			_cashService = new CashService();
			_env = env;
		}

		[HttpGet()]
		public List<CashDocumentType> GetCashDocumentList()
		{
			return _cashService.CashDocumentList();
		}

		[HttpPost()]
		public async Task<AppResult> AddDocument(IFormFile? image, [FromForm] int employeeId, [FromForm] string docDate, [FromForm] string amount, [FromForm] string description, [FromForm] int docType)
		{
			DateTime documentDate = Convert.ToDateTime(docDate);

			var _amount = Convert.ToDouble(amount.Replace("$", "").Replace(".", "").Replace(",", "")) / 100;

			var uploads = Path.Combine(_env.ContentRootPath, "Documents");
			var filePath = string.Empty;

			if (image?.Length > 0)
			{
				filePath = Path.Combine(uploads, image.FileName);

				using (var stream = System.IO.File.Create(filePath))
				{
					await image.CopyToAsync(stream);
				}
			}

			return _cashService.AddDocument(image?.FileName, employeeId, documentDate, _amount, description, docType, filePath);
		}


		[HttpGet()]
		public List<DayResultState> GetDayResultState()
		{
			return _cashService.GetDayResultState();
		}

		[HttpGet()]
		public DayResultModel GetDayResult()
		{
			return _cashService.GetDayResult();
		}

		[HttpGet()]
		public string GetCashDrawer()
		{
			return _cashService.GetCashDrawer();
		}



		[HttpGet()]
		public List<PosActionModel> GetActions()
		{
			return _cashService.GetActions();
		}

		[HttpPost()]
		public async Task<AppResult> UpdateDayResult(IFormFile? image, [FromForm] int resultId, [FromForm] int stateId, [FromForm] string description, [FromForm] int employeeId, [FromForm] string docDate)
		{
			DateTime updateDate = Convert.ToDateTime(docDate);

			var uploads = Path.Combine(_env.ContentRootPath, "Documents");
			var filePath = string.Empty;

			if (image?.Length > 0)
			{
				filePath = Path.Combine(uploads, image.FileName);

				using (var stream = System.IO.File.Create(filePath))
				{
					await image.CopyToAsync(stream);
				}
			}

			return _cashService.UpdateDayResult(image?.FileName, resultId, stateId, description, employeeId, updateDate, filePath);

		}



	}
}
