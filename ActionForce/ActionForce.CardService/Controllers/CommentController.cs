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
    public class CommentController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage AddComment(string comment, short direction) // 0 request, 1 response
        {
            Result result = new Result();

            result.IsSuccess = false;
            result.Message = string.Empty;

            if (!string.IsNullOrEmpty(comment))
            {
                var infolist = comment.Split(';').ToArray();
                var serial = infolist[0];
                var macadd = infolist[1];
                var proces = infolist[2];

                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {
                    var parameters = new { SerialNumber = serial, MacAddress = macadd, Direction = direction, ProcessNumber = proces, Comment = comment, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                    var sql = "INSERT INTO [dbo].[NFCCardComments] ([SerialNumber],[MacAddress],[Direction],[ProcessNumber],[Comment],[RecordIP],[RecordDate]) VALUES(@SerialNumber, @MacAddress, @Direction, @ProcessNumber ,@Comment, @IP, @Date)";
                    connection.Execute(sql, parameters);
                }

                result.IsSuccess = true;
                result.Message = $"OK";
            }
            result.ProcessDate = DateTime.UtcNow.AddHours(3);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}