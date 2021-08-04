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
        [HttpGet]
        public HttpResponseMessage GetSummary() //(Header Header_Info, string AdisyonNo, string SerialNo)
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Tamam");
        }

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

                            var ticketlist = Db.VAdisyonSummary.Where(x => x.LocationID == POSTerminalInfo.LocationID && x.SaleDate == localdate).ToList();

                            result.ResultCode = 0;
                            result.ResultMessage = $"{ticketlist.Count} adet adisyon bulundu";
                            result.SummaryList = ticketlist.Select(x => new AdisyonSummary()
                            {
                                AdisyonID = x.ID,
                                AdisyonNo = x.ID.ToString(),
                                AdisyonName = POSTerminalInfo.LocationFullName,
                                TableNo = x.LocationID.ToString(),
                                NetAmount = Convert.ToInt64(x.Total * 100),
                                TotalAmount = Convert.ToInt64(x.Total * 100)
                            }).ToList();

                        }
                        else if (!string.IsNullOrEmpty(request.AdisyonNo) && request.AdisyonNo != "0")
                        {
                            var ticketlist = Db.VAdisyonSummary.Where(x => x.LocationID == POSTerminalInfo.LocationID && x.SaleDate == localdate && x.ID.ToString() == request.AdisyonNo).ToList();

                            if (ticketlist != null && ticketlist.Count > 0)
                            {
                                result.ResultCode = 0;
                                result.ResultMessage = $"{ticketlist.Count} adet adisyon bulundu";
                                result.SummaryList = ticketlist.Select(x => new AdisyonSummary()
                                {
                                    AdisyonID = x.ID,
                                    AdisyonNo = x.ID.ToString(),
                                    AdisyonName = POSTerminalInfo.LocationFullName,
                                    TableNo = x.LocationID.ToString(),
                                    NetAmount = Convert.ToInt64(x.Total * 10),
                                    TotalAmount = Convert.ToInt64(x.Total * 10)
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
                        result.ResultMessage = $"terminal bilgisi bulunamadı";
                        result.SummaryList = null;
                    }
                }
                else
                {
                    result.ResultCode = 2;
                    result.ResultMessage = $"servis kullanıcısı bulunamadı";
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
            ApiHelper.AddPosServiceLog(LocationID, request.SerialNo, request.AdisyonNo, request.Header_Info.UserName, ApiHelper.PasswordMD5_Pan(request.Header_Info.Password), "TSM_IR_GetAdisyonSummary", result.ResultCode.ToString(), result.ResultMessage);

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

                            var ticket = Db.TicketSaleRows.FirstOrDefault(x => x.LocationID == POSTerminalInfo.LocationID && x.ID == request.AdisyonId);

                            if (ticket != null)
                            {
                                result.ResultCode = 0;
                                result.TicketTypeChangeDisabled = 1;
                                result.ResultMessage = $"Adisyon Detayı";

                                var detail = new AdisyonDetail();

                                detail.CheckNo = ticket.ID.ToString();
                                detail.Address1 = string.Empty;
                                detail.Address2 = string.Empty;
                                detail.AdisyonName = POSTerminalInfo.LocationFullName;
                                detail.TicketType = 1;
                                detail.InvoiceInfo = null;
                                detail.CurrentAccountInfo = null;
                                detail.SalesItemizerList.Add(new SalesItemizerList() { 
                                
                                });




                            }

                        }
                        else
                        {
                           
                        }
                    }
                    else
                    {
                        
                    }
                }
                else
                {
                    
                }

            }
            catch (Exception ex)
            {
                
            }

            // Log Alma
            ApiHelper.AddPosServiceLog(LocationID, request.SerialNo, request.AdisyonId.ToString(), request.Header_Info.UserName, ApiHelper.PasswordMD5_Pan(request.Header_Info.Password), "TSM_IR_GetAdisyonSummary", result.ResultCode.ToString(), result.ResultMessage);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }
    }
}
