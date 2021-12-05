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
    public class CardReaderController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage CardReaderInfo(string info)
        {
            ParameterResult result = new ParameterResult();
            ServiceHelper helper = new ServiceHelper();

            result.IsSuccess = false;
            result.Message = string.Empty;

            

            //00175D8B;CC:50:E3:17:5D:8B;2.22;100;100;1;2
            //Serino, macadresi, versiyon, ucret, milisaniye, tetik sayısı, bekleme süresi

            if (!string.IsNullOrEmpty(info))
            {
                var infolist = info.Split(';').ToArray();

                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {
                    var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                    var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'CardReader', 'CardReaderInfo', 'String')";
                    connection.Execute(sql, parameters);
                }

                try
                {
                    CardReaderParameterModel model = new CardReaderParameterModel();

                    model.SerialNumber = infolist[0];
                    model.MACAddress = infolist[1];
                    model.Version = infolist[2];
                    model.UnitPrice = Convert.ToInt32(infolist[3]);
                    model.MiliSecond = Convert.ToInt32(infolist[4]);
                    model.ReadCount = Convert.ToInt32(infolist[5]);
                    model.UnitDuration = Convert.ToInt32(infolist[6]);

                    var addResult = helper.AddCardReaderParameter(model);

                    result = addResult;
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.Message = $"ERR";
                }
            }

            result.ProcessDate = DateTime.UtcNow.AddHours(3);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}
