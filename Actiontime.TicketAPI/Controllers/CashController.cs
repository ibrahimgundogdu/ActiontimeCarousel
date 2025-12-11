using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CashController : ControllerBase
    {
        CashService _cashService;
        CloudService _cloudService;

        private IWebHostEnvironment _env;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextFactory<ApplicationCloudDbContext> _cdbFactory;

        public CashController(IWebHostEnvironment env, IDbContextFactory<ApplicationDbContext> dbFactory, IDbContextFactory<ApplicationCloudDbContext> cdbFactory, CloudService cloudService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _cdbFactory = cdbFactory ?? throw new ArgumentNullException(nameof(cdbFactory));
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));
            _cashService = new CashService(_dbFactory, _cdbFactory, _cloudService);
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
