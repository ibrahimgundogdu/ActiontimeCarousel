using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ActionForce.CardService.Controllers
{
    public class PersonalController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage CardRead(string info)
        {
            Result result = new Result();
            ServiceHelper helper = new ServiceHelper();

            result.IsSuccess = true;
            result.Message = $"OK";
            result.ProcessDate = DateTime.UtcNow.AddHours(3);

            //00175D8B;CC:50:E3:17:5D:8B;4528C2G3;19

            if (!string.IsNullOrEmpty(info))
            {
                var infolist = info.Split(';').ToArray();

                var IslemNo = infolist[3];
                result.ProcessNumber = IslemNo;

                CardReadPersonalModel model = new CardReadPersonalModel();

                model.SerialNumber = infolist[0];
                model.MACAddress = infolist[1];
                model.CardNumber = infolist[2];
                model.EmployeeID = Convert.ToInt32(infolist[3]);

                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {
                    var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                    var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'Default', 'GetPersonalCardInfo', 'String')";
                    connection.Execute(sql, parameters);
                }

                var addResult = helper.AddPersonalCardAction(model);

                result.IsSuccess = addResult.IsSuccess;
                result.Message = addResult.Message;

            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
