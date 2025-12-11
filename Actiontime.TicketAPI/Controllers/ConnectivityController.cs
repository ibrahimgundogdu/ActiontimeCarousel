using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConnectivityController : ControllerBase
    {
        ConnectivityService _connectivityService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        public ConnectivityController(IDbContextFactory<ApplicationDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));

            _connectivityService = new ConnectivityService(_dbFactory);
        }

        [HttpGet()]
        public bool Ping()
        {
            return true;
        }

        [HttpGet()]
        public OurLocation GetLocation()
        {
            return _connectivityService.GetOurLocation();
        }


    }
}
