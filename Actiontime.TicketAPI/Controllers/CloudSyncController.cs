using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CloudSyncController : ControllerBase
    {
        [HttpGet("{Id}/{employeeId}")]
        public string GetLocationParts(string Id, string employeeId)
        {
            return "value";
        }
    }
}
