using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Services;
using Actiontime.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConnectivityController : ControllerBase
    {
        private readonly IConnectivityService _connectivityService;

        public ConnectivityController(IConnectivityService connectivityService)
        {
            _connectivityService = connectivityService;
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
