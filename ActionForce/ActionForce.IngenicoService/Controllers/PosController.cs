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
            DateTime localdate = DateTime.UtcNow.AddHours(3);

            try
            {
                var isAuthentication = ApiHelper.CheckUserAuthentication(request.Header_Info);

                if (isAuthentication)
                {

                    var POSTerminalInfo = Db.VLocationPosTerminal.FirstOrDefault(x => x.SerialNumber == request.SerialNo && x.IsActive == true && x.IsMaster == true);

                    if (POSTerminalInfo != null)
                    {
                        if (!string.IsNullOrEmpty(request.AdisyonNo) && request.AdisyonNo == "0")
                        {

                            var ticketlist = Db.VAdisyonSummary.Where(x => x.LocationID == POSTerminalInfo.LocationID && x.SaleDate == localdate).ToList();

                            result.ResultCode = 0;
                            result.ResultMessage = $"{ticketlist.Count} adet adisyon bulundu";
                            result.SummaryList = ticketlist.Select(x => new AdisyonSummary()
                            {
                                AdisyonID = x.ID,
                                AdisyonNo = x.UID.ToString(),
                                AdisyonName = POSTerminalInfo.LocationFullName,
                                TableNo = x.LocationID.ToString(),
                                NetAmount = Convert.ToInt64(x.Total * 10),
                                TotalAmount = Convert.ToInt64(x.Total * 10)
                            }).ToList();
                        }
                        else if (!string.IsNullOrEmpty(request.AdisyonNo) && request.AdisyonNo != "0")
                        {
                            var ticketlist = Db.VAdisyonSummary.Where(x => x.LocationID == POSTerminalInfo.LocationID && x.SaleDate == localdate && x.UID.ToString() == request.AdisyonNo).ToList();

                            if (ticketlist != null && ticketlist.Count > 0)
                            {
                                result.ResultCode = 0;
                                result.ResultMessage = $"{ticketlist.Count} adet adisyon bulundu";
                                result.SummaryList = ticketlist.Select(x => new AdisyonSummary()
                                {
                                    AdisyonID = x.ID,
                                    AdisyonNo = x.UID.ToString(),
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

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }
    }
}
