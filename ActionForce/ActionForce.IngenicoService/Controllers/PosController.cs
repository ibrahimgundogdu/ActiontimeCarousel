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

        [HttpPost]
        public HttpResponseMessage TSM_IR_GetAdisyonSummary([FromBody] SummaryRequest request)
        {
            ResultSummary result = new ResultSummary();
            DateTime localdate = DateTime.UtcNow.AddHours(3).Date;
            int LocationID = 0;

            try
            {
                var isAuthentication = ApiHelper.CheckUserAuthentication(request.Header_Info);

                if (isAuthentication)
                {

                    var POSTerminalInfo = Db.VLocationPosTerminal.FirstOrDefault(x => x.SerialNumber == request.SerialNo && x.IsActive == true && x.IsMaster == true);

                    if (POSTerminalInfo != null)
                    {
                        LocationID = POSTerminalInfo.LocationID;

                        if (!string.IsNullOrEmpty(request.AdisyonNo) && request.AdisyonNo == "0")
                        {

                            var ticketlist = Db.VAdisyonSummary.Where(x => x.LocationID == POSTerminalInfo.LocationID && x.Date == localdate).ToList();

                            result.ResultCode = 0;
                            result.ResultMessage = $"{ticketlist.Count} adet adisyon bulundu";
                            result.SummaryList = ticketlist.Select(x => new AdisyonSummary()
                            {
                                AdisyonID = x.ID,
                                AdisyonNo = x.OrderNumber.ToString(),
                                AdisyonName = POSTerminalInfo.LocationFullName,
                                TableNo = x.LocationID.ToString(),
                                NetAmount = Convert.ToInt64(x.Amount * 100),
                                TotalAmount = Convert.ToInt64(x.Amount * 100)
                            }).ToList();

                        }
                        else if (!string.IsNullOrEmpty(request.AdisyonNo) && request.AdisyonNo != "0")
                        {
                            var ticketlist = Db.VAdisyonSummary.Where(x => x.LocationID == POSTerminalInfo.LocationID && x.Date == localdate && x.ID.ToString() == request.AdisyonNo).ToList();

                            if (ticketlist != null && ticketlist.Count > 0)
                            {
                                result.ResultCode = 0;
                                result.ResultMessage = $"{ticketlist.Count} adet adisyon bulundu";
                                result.SummaryList = ticketlist.Select(x => new AdisyonSummary()
                                {
                                    AdisyonID = x.ID,
                                    AdisyonNo = x.OrderNumber.ToString(),
                                    AdisyonName = POSTerminalInfo.LocationFullName,
                                    TableNo = x.LocationID.ToString(),
                                    NetAmount = Convert.ToInt64(x.Amount * 10),
                                    TotalAmount = Convert.ToInt64(x.Amount * 10)
                                }).ToList();
                            }
                            else
                            {
                                result.ResultCode = 2;
                                result.ResultMessage = $"adisyon bulunamadı";
                                result.SummaryList = null;
                            }
                        }
                        else
                        {
                            result.ResultCode = 2;
                            result.ResultMessage = $"adisyon no gelmedi";
                            result.SummaryList = null;
                        }
                    }
                    else
                    {
                        result.ResultCode = 2;
                        result.ResultMessage = $"POS Terminal bilgisi bulunamadı";
                        result.SummaryList = null;
                    }
                }
                else
                {
                    result.ResultCode = 2;
                    result.ResultMessage = $"Servis kullanıcısı bulunamadı";
                    result.SummaryList = null;
                }

            }
            catch (Exception ex)
            {
                result.ResultCode = 1;
                result.ResultMessage = $"sistem hatası oluştu : " + ex.Message;
                result.SummaryList = null;
            }

            // Log Alma
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            data = data.Replace(request.Header_Info.UserName, "*").Replace(request.Header_Info.Password, "*");

            ApiHelper.AddPosServiceLog(LocationID, request.SerialNo, request.AdisyonNo, request.Header_Info.UserName, ApiHelper.PasswordMD5_Pan(request.Header_Info.Password), "TSM_IR_GetAdisyonSummary", data, result.ResultCode.ToString(), result.ResultMessage);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }

        [HttpPost]
        public HttpResponseMessage TSM_IR_GetAdisyon([FromBody] DetailRequest request)
        {
            ResultDetail result = new ResultDetail();
            int LocationID = 0;

            try
            {
                var isAuthentication = ApiHelper.CheckUserAuthentication(request.Header_Info);

                if (isAuthentication)
                {

                    var POSTerminalInfo = Db.VLocationPosTerminal.FirstOrDefault(x => x.SerialNumber == request.SerialNo && x.IsActive == true && x.IsMaster == true);

                    if (POSTerminalInfo != null)
                    {
                        LocationID = POSTerminalInfo.LocationID;

                        if (request.AdisyonId != 0)
                        {

                            var order = Db.TicketSale.FirstOrDefault(x => x.LocationID == POSTerminalInfo.LocationID && x.ID == request.AdisyonId);

                            if (order != null)
                            {
                                result.ResultCode = 0;
                                result.TicketTypeChangeDisabled = 1;
                                result.ResultMessage = $"Adisyon Detayı";

                                var detail = new AdisyonDetail();

                                detail.CheckNo = order.ID.ToString();
                                detail.Address1 = string.Empty;
                                detail.Address2 = string.Empty;
                                detail.AdisyonName = POSTerminalInfo.LocationFullName;
                                detail.TicketType = 1;
                                detail.InvoiceInfo = null;
                                detail.CurrentAccountInfo = null;
                                detail.SalesItemizerList = null;

                                detail.SaleItemList = new List<SaleItem>();
                                var orderrows = Db.VTicketSaleRowsAll.Where(x => x.OrderID == order.ID).ToList();
                                foreach (var item in orderrows)
                                {
                                    detail.SaleItemList.Add(new SaleItem()
                                    {
                                        Quantity = item.Quantity,
                                        QuantityType = 1,
                                        TaxRate = Convert.ToInt32(item.TaxRate * 100),
                                        Title = item.ProductName,
                                        UnitAmount = Convert.ToInt64(item.RowTotal * 100)
                                    });
                                }


                                detail.PaymentList = null;
                                detail.DiscountList = null;
                                detail.UserMessageList = null;

                                result.Detail = detail;
                            }
                            else
                            {
                                result.ResultCode = 2;
                                result.TicketTypeChangeDisabled = 1;
                                result.ResultMessage = $"Adisyon Satırı Bulanamadı";
                                result.Detail = null;
                            }

                        }
                        else
                        {
                            result.ResultCode = 2;
                            result.TicketTypeChangeDisabled = 1;
                            result.ResultMessage = $"Adisyon Bulanamadı";
                            result.Detail = null;
                        }
                    }
                    else
                    {
                        result.ResultCode = 2;
                        result.TicketTypeChangeDisabled = 1;
                        result.ResultMessage = $"POS Terminal Bilgisi Bulanamadı";
                        result.Detail = null;
                    }
                }
                else
                {
                    result.ResultCode = 2;
                    result.TicketTypeChangeDisabled = 1;
                    result.ResultMessage = $"Servis kullanıcısı bulunamadı";
                    result.Detail = null;

                }

            }
            catch (Exception ex)
            {
                result.ResultCode = 1;
                result.TicketTypeChangeDisabled = 1;
                result.ResultMessage = $"Sistem hatası oluştu";
                result.Detail = null;
            }

            // Log Alma
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            data = data.Replace(request.Header_Info.UserName, "*").Replace(request.Header_Info.Password, "*");
            ApiHelper.AddPosServiceLog(LocationID, request.SerialNo, request.AdisyonId.ToString(), request.Header_Info.UserName, ApiHelper.PasswordMD5_Pan(request.Header_Info.Password), "TSM_IR_GetAdisyon", data, result.ResultCode.ToString(), result.ResultMessage);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }

        [HttpPost]
        public HttpResponseMessage TSM_lR_SendAdisyonPayment([FromBody] SendAdisyonPaymentRequest request)
        {
            ResultPayment result = new ResultPayment();
            int LocationID = 0;

            try
            {
                var isAuthentication = ApiHelper.CheckUserAuthentication(request.Header_Info);

                if (isAuthentication)
                {

                    var POSTerminalInfo = Db.VLocationPosTerminal.FirstOrDefault(x => x.SerialNumber == request.SerialNo && x.IsActive == true && x.IsMaster == true);

                    if (POSTerminalInfo != null)
                    {
                        LocationID = POSTerminalInfo.LocationID;

                        if (request.AdisyonId != 0)
                        {

                            var order = Db.TicketSale.FirstOrDefault(x => x.LocationID == POSTerminalInfo.LocationID && x.ID == request.AdisyonId);

                            if (order != null)
                            {

                                //receipt kaydı













                                result.ResultCode = 0;
                                result.ResultMessage = $"Adisyon Odeme Bilgisi Alındı";

                                // buraya kodlar gelecek
                            }
                            else
                            {
                                result.ResultCode = 2;
                                result.ResultMessage = $"Adisyon Bulanamadı";
                            }

                        }
                        else
                        {
                            result.ResultCode = 2;
                            result.ResultMessage = $"Adisyon Bulanamadı";
                        }
                    }
                    else
                    {
                        result.ResultCode = 2;
                        result.ResultMessage = $"POS Terminal Bilgisi Bulanamadı";
                    }
                }
                else
                {
                    result.ResultCode = 2;
                    result.ResultMessage = $"Servis kullanıcısı bulunamadı";
                }

            }
            catch (Exception ex)
            {
                result.ResultCode = 1;
                result.ResultMessage = $"Sistem hatası oluştu.";
            }

            // Log Alma
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            data = data.Replace(request.Header_Info.UserName, "*").Replace(request.Header_Info.Password, "*");
            ApiHelper.AddPosServiceLog(LocationID, request.SerialNo, request.AdisyonId.ToString(), request.Header_Info.UserName, ApiHelper.PasswordMD5_Pan(request.Header_Info.Password), "TSM_lR_SendAdisyonPayment", data, result.ResultCode.ToString(), result.ResultMessage);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }

        [HttpPost]
        public HttpResponseMessage TSM_IR_SetStatus([FromBody] SetStatusRequest request)
        {
            ResultStatus result = new ResultStatus();
            int LocationID = 0;

            try
            {
                var isAuthentication = ApiHelper.CheckUserAuthentication(request.Header_Info);

                if (isAuthentication)
                {

                    var POSTerminalInfo = Db.VLocationPosTerminal.FirstOrDefault(x => x.SerialNumber == request.SerialNo && x.IsActive == true && x.IsMaster == true);

                    if (POSTerminalInfo != null)
                    {
                        LocationID = POSTerminalInfo.LocationID;

                        if (request.AdisyonId != 0)
                        {

                            var order = Db.TicketSale.FirstOrDefault(x => x.LocationID == POSTerminalInfo.LocationID && x.ID == request.AdisyonId);

                            if (order != null)
                            {

                                try
                                {
                                    Db.SetTicketSaleStatus(order.ID, request.Status);

                                    result.ResultCode = 0;
                                    result.ResultMessage = $"Adisyon Durumu İşlendi";
                                }
                                catch (Exception ex)
                                {
                                    result.ResultCode = 1;
                                    result.ResultMessage = $"Adisyon Durumu İşlenemedi";
                                }


                                // buraya kodlar gelecek
                            }
                            else
                            {
                                result.ResultCode = 2;
                                result.ResultMessage = $"Adisyon Bulanamadı";
                            }

                        }
                        else
                        {
                            result.ResultCode = 2;
                            result.ResultMessage = $"Adisyon Bulanamadı";
                        }
                    }
                    else
                    {
                        result.ResultCode = 2;
                        result.ResultMessage = $"POS Terminal Bilgisi Bulanamadı";
                    }
                }
                else
                {
                    result.ResultCode = 2;
                    result.ResultMessage = $"Servis kullanıcısı bulunamadı";
                }

            }
            catch (Exception ex)
            {
                result.ResultCode = 1;
                result.ResultMessage = $"Sistem hatası oluştu.";
            }

            string data = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            data = data.Replace(request.Header_Info.UserName, "*").Replace(request.Header_Info.Password, "*");
            ApiHelper.AddPosServiceLog(LocationID, request.SerialNo, request.AdisyonId.ToString(), request.Header_Info.UserName, ApiHelper.PasswordMD5_Pan(request.Header_Info.Password), "TSM_lR_SendAdisyonPayment", data, result.ResultCode.ToString(), result.ResultMessage);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }
    }
}
