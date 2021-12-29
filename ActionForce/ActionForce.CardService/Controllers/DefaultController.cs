using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ActionForce.CardService.Controllers
{

    public class DefaultController : ApiController
    {

        [HttpGet]
        public HttpResponseMessage GetCardInfo(string info)
        {
            Result result = new Result();
            ServiceHelper helper = new ServiceHelper();

            result.IsSuccess = false;
            result.Message = string.Empty;
            result.ProcessDate = DateTime.UtcNow.AddHours(3);

            if (!string.IsNullOrEmpty(info))
            {
                var infolist = info.Split(';').ToArray();

                var IslemNo = infolist[3];
                result.ProcessNumber = IslemNo;

                CardReadModel model = new CardReadModel();
                //00175D8B;CC:50:E3:17:5D:8B;3;2;0;4528C2F3;100;169700;1637732352
                var epoc = Convert.ToDouble(infolist[8]);
                DateTime epocdate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoc);

                model.SerialNumber = infolist[0];
                model.MACAddress = infolist[1];
                model.ProcessType = Convert.ToInt16(infolist[2]);
                model.ProcessNumber = Convert.ToInt32(infolist[3]);
                model.CardType = Convert.ToInt16(infolist[4]);
                model.CardNumber = infolist[5];
                model.RidePrice = Convert.ToDouble(infolist[6]) / 100;
                model.CardBlance = Convert.ToDouble(infolist[7]) / 100;
                model.ProcessDate = epocdate;

                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {
                    var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                    var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'Default', 'GetCardAction', 'String')";
                    connection.Execute(sql, parameters);
                }

                var addResult = helper.AddCardAction(model);

                result.IsSuccess = true;
                result.Message = addResult.Message;
            }
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        public HttpResponseMessage GetDateInfo()
        {
            DateInfo dateInfo = new DateInfo();

            var CurrentDate = DateTime.UtcNow.AddHours(3);

            dateInfo.DateTime = $"{CurrentDate.Year}-{CurrentDate.Month}-{CurrentDate.Day} {CurrentDate.Hour}:{CurrentDate.Minute}:{CurrentDate.Second}";


            using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
            {
                var parameters = new { Message = CurrentDate.ToString(), IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'Default', 'GetDateInfo', '')";
                var places = connection.Execute(sql, parameters);
            }

            return Request.CreateResponse(HttpStatusCode.OK, dateInfo);

        }
    }
}