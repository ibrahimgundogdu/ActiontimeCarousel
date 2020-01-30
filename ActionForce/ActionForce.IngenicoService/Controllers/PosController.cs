using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ActionForce.PosService.Controllers
{
    public class PosController : BaseController
    {
        [PosAuthorization]
        [HttpPost]
        public HttpResponseMessage TSM_IR_GetAdisyonSummary(string AdisyonNo, string SerialNo)
        {
            Result result = new Result();

            //



            return Request.CreateResponse(HttpStatusCode.OK, result);

        }
    }
}
