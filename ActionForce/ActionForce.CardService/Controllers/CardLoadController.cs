using ActionForce.CardService.Models;
using ActionForce.CardService.PushService;
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
    public class CardLoadController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage CardInfo(string info)
        {
            Result result = new Result();
            //ServiceHelper helper = new ServiceHelper();

            result.IsSuccess = false;
            result.Message = string.Empty;

            //00119D9B;CC:50:E3:11:9D:9B;4528C2F3;100
            //Serino, macadresi, card no, bakiye

            if (!string.IsNullOrEmpty(info))
            {
                var infolist = info.Split(';').ToArray();

                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {
                    var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                    var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'CardInfo', 'String')";
                    connection.Execute(sql, parameters);
                }

                try
                {
                    CardLoadInfoModel model = new CardLoadInfoModel();

                    model.SerialNumber = infolist[0];
                    model.MACAddress = infolist[1];
                    model.CardNumber = infolist[2];
                    model.CardBlance = (Convert.ToDouble(infolist[3]) / 100);

                    using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                    {
                        var rparameters = new { SerialNumber = model.SerialNumber, MACAddress = model.MACAddress };
                        var rsql = @"SELECT TOP(1) [ID]
                                ,[OurCompanyID]
                                ,[LocationID]
                                ,[LocationTypeID]
                                ,[CardReaderTypeID]
                                ,[LocationPartID]
                                ,[PartName]
                                ,[PartGroupName]
                                ,[SerialNumber]
                                ,[MACAddress]
                                ,[UID]
                                ,[StartDate]
                                ,[IsActive]
                                FROM [dbo].[CardReader] Where [SerialNumber] = @SerialNumber and [MACAddress] = @MACAddress";
                        CardReader cardReader = connection.QueryFirstOrDefault<CardReader>(rsql, rparameters);

                        if (cardReader != null && cardReader.LocationID != null)
                        {
                            PushClient pushService = new PushClient();
                            pushService.SendCardInfo(cardReader.LocationID ?? 0, info);
                        }
                    }

                    result.IsSuccess = true;
                    result.Message = "OK";
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

        [HttpGet]
        public HttpResponseMessage CardLoadInfo(Guid? id)
        {
            ServiceHelper helper = new ServiceHelper();

            if (id != null)
            {
                try
                {
                    using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                    {
                        var parameters = new { Message = id.ToString(), IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                        var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'CardLoadInfo', 'Guid')";
                        connection.Execute(sql, parameters);

                        var cardLoadInfo = helper.GetCardLoad(id.Value);

                        if (cardLoadInfo != null)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, cardLoadInfo);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

        }

        [HttpGet]
        public HttpResponseMessage CardLoadResult(Guid? id, int? success, string Message)
        {
            Result result = new Result();
            ServiceHelper helper = new ServiceHelper();
            PushClient pushService = new PushClient();

            result.IsSuccess = false;
            result.Message = string.Empty;

            if (id != null && success != null)
            {
                string info = $"id:{id} success:{success} message:{Message}";
                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {
                    var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3) };
                    var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'CardLoadResult', 'Guid,int')";
                    connection.Execute(sql, parameters);

                    var mparameter = new { UID = id, Message };
                    var msql = "Update [dbo].[TicketSaleCreditLoad] Set [Message] = @Message Where [UID] = @UID";
                    connection.Execute(msql, mparameter);
                }

                CardCreditLoad cardReader = helper.GetCardLoad(id.Value);

                try
                {
                   
                    if (success == 1) // Yükleme başarılı şekilde gerçekleşti ise
                    {
                        var loadResult = helper.LoadCard(cardReader);

                        result.IsSuccess = loadResult.IsSuccess;
                        result.Message = loadResult.Message;
                    }
                    
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.Message = $"ERR";
                }

                pushService.SendCardLoadResult(cardReader.LocationID, id.ToString());
            }

            

            result.ProcessDate = DateTime.UtcNow.AddHours(3);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }



    }
}
