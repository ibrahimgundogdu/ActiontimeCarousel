using Actiontime.Data.Entities;
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
        public ConnectivityController()
        {
            _connectivityService = new ConnectivityService();
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
