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
        public HttpResponseMessage ReceiveComment(string comment)
        {
            Result result = new Result();
            ServiceHelper helper = new ServiceHelper();
            PushClient pushService = new PushClient();

            result.IsSuccess = false;
            result.Message = string.Empty;

            if (!string.IsNullOrEmpty(comment))
            {
                try
                {
                    var infolist = comment.Split(';').ToArray();

                    var SerialNumber = infolist[0];
                    var MACAddress = infolist[1];
                    var Process = infolist[2];


                    using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                    {
                        var rparameters = new { SerialNumber, MACAddress };
                        var rsql = @"SELECT TOP(1) * FROM [dbo].[CardReader] Where [SerialNumber] = @SerialNumber and [MACAddress] = @MACAddress";
                        CardReader cardReader = connection.QueryFirstOrDefault<CardReader>(rsql, rparameters);

                        if (cardReader != null && cardReader.LocationID != null)
                        {
                            if (Process == "1")
                            {
                                if (infolist.Length >= 5 && infolist[4] == "7") //kontör yükleme başarılı
                                {
                                    if (!string.IsNullOrEmpty(cardReader.Version) && cardReader.Version == "1.12")
                                    {
                                        //00DD4D48;E8:DB:84:DD:4D:48;1;194FBD8D;7;10000;1779

                                        var CardNumber = infolist[3];
                                        var crParameters = new { SerialNumber, MACAddress, CardNumber };
                                        var crSql = "Exec [dbo].[CheckCreditLoad] @SerialNumber , @MACAddress, @CardNumber";
                                        var creditLoadID = connection.QueryFirst(crSql, crParameters);  // id si alınır
                                        var creditLoad = helper.GetCardLoad(creditLoadID.ID);
                                        var loadresult = CardLoadResult(creditLoad.UID, 1, "Kredi Yüklendi");

                                    }
                                    else
                                    {
                                        var creditLoadID = Convert.ToInt32(infolist[6]);
                                        var creditLoad = helper.GetCardLoad(creditLoadID);
                                        var loadresult = CardLoadResult(creditLoad.UID,1,"Kredi Yüklendi"); //helper.LoadCard(creditLoad);
                                    }
                                    
                                }

                            }
                            else if (Process == "80")
                            {
                                var Version = infolist[3];
                                var IPAdress = infolist[4];

                                var crParameters = new { SerialNumber , MACAddress , Version, IPAdress };
                                var crSql = "Exec [dbo].[CheckCardReader] @SerialNumber , @MACAddress, @Version, @IPAdress";

                                connection.Query(crSql, crParameters);

                            }

                            pushService.SendComment(cardReader.LocationID ?? 0, comment);

                            result.IsSuccess = true;
                            result.Message = "OK";
                        }
                        else
                        {
                            var Version = infolist[3];
                            var IPAdress = infolist[4];

                            var crParameters = new { SerialNumber, MACAddress, Version, IPAdress };
                            var crSql = "Exec [dbo].[CheckCardReader] @SerialNumber , @MACAddress, @Version, @IPAdress";

                            connection.Query(crSql, crParameters);

                            return ReceiveComment(comment);
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.Message = $"ERR";
                }

            }

            using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
            {
                var parameters = new { Message = comment, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3), ResultMessage = result.Message };
                var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [ResponseMessage] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'ReceiveComment', @ResultMessage)";
                connection.Execute(sql, parameters);
            }

            result.ProcessDate = DateTime.UtcNow.AddHours(3);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        public HttpResponseMessage CardInfo(string info)
        {
            Result result = new Result();

            result.IsSuccess = false;
            result.Message = string.Empty;

            if (!string.IsNullOrEmpty(info))
            {
                var infolist = info.Split(';').ToArray();

                try
                {
                    CardLoadInfoModel model = new CardLoadInfoModel();

                    model.SerialNumber = infolist[0];
                    model.MACAddress = infolist[1];
                    model.Process = Convert.ToInt32(infolist[2]);

                    using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                    {
                        var rparameters = new { SerialNumber = model.SerialNumber, MACAddress = model.MACAddress };
                        var rsql = @"SELECT TOP(1) * FROM [dbo].[CardReader] Where [SerialNumber] = @SerialNumber and [MACAddress] = @MACAddress";
                        CardReader cardReader = connection.QueryFirstOrDefault<CardReader>(rsql, rparameters);

                        if (cardReader != null && cardReader.LocationID != null)
                        {
                            PushClient pushService = new PushClient();
                            pushService.SendCardInfo(cardReader.LocationID ?? 0, info);

                            result.IsSuccess = true;
                            result.Message = "OK";
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = "READERNOTFOUND";
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.IsSuccess = false;
                    result.Message = $"ERR";
                }

            }

            using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
            {
                var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3), ResultMessage = result.Message };
                var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [ResponseMessage] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'CardInfo', @ResultMessage)";
                connection.Execute(sql, parameters);
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
            string info = $"id:{id} success:{success} message:{Message}";

            if (id != null && success != null)
            {
                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {

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

            using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
            {
                var parameters = new { Message = info, IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3), ResultMessage = result.Message };
                var sql = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [Module], [ResponseMessage] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'CardLoadResult', '', @ResultMessage)";
                connection.Execute(sql, parameters);
            }

            result.ProcessDate = DateTime.UtcNow.AddHours(3);
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [HttpGet]
        public HttpResponseMessage CardServiceLog(Guid? id)
        {

            if (id != null && id.ToString().ToUpper() == "DC27522E-7828-4253-AADC-3557CD082DAE")
            {
                using (var connection = new SqlConnection(ServiceHelper.GetConnectionString()))
                {

                    var parameters = new { Message = "LOG", IP = ServiceHelper.GetIPAddress(), Date = DateTime.UtcNow.AddHours(3), ResultMessage = "OK" };
                    var sqll = "INSERT INTO [dbo].[NFCCardLog] ([Message], [RecordIP], [RecordDate], [Controller], [Action], [ResponseMessage] ) VALUES(@Message,@IP, @Date, 'CardLoad', 'ReceiveComment', @ResultMessage)";
                    connection.Execute(sqll, parameters);

                    var sql = "SELECT TOP (200) * FROM [dbo].[NFCCardLog] WHERE [Message] <> 'LOG' ORDER BY ID DESC";
                    var logs = connection.Query<CardLog>(sql).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, logs);

                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }


        }

    }
}
