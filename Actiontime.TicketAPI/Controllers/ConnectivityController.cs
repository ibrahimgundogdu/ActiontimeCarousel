using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConnectivityController : ControllerBase
    {
        ConnectivityService _connectivityService;
        private readonly ApplicationDbContext _db;
        public ConnectivityController(ApplicationDbContext db)
        {
            _connectivityService = new ConnectivityService(db);
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
