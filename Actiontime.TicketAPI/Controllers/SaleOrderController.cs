using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Actiontime.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SaleOrderController : ControllerBase
    {
        private readonly ISaleOrderService _orderService;
        private readonly ICloudService _cloudService;


        public SaleOrderController(ICloudService cloudService, ISaleOrderService saleOrderService)
        {
            _cloudService = cloudService;
            _orderService = saleOrderService;
        }



        [HttpPost()]
        public bool AddBasket([FromBody] Basket basket)
        {
            return _orderService.AddBasket(basket);
        }


        [HttpPost()]
        public AddOrderResult AddOrder([FromBody] AddOrderRequest request)
        {
            var result = _orderService.AddOrder(request.token, request.paymethodId, request.appBasketItems);

            return result;

        }

        [HttpGet()]
        public TicketReceipt GetReceipt(int orderId)
        {
            return _orderService.GetReceipt(orderId);
        }

        [HttpGet()]
        public TicketCheck GetTicket(string qrcode)
        {
            return _orderService.GetTicket(qrcode);
        }


        //Ticket operations
        [HttpGet()]
        public bool ConfirmTicket(string qrcode)
        {
            return false; //return _orderService.GetTicket(qrcode);
        }

        [HttpGet()]
        public bool BackTicket(string qrcode)
        {
            return false; //return _orderService.GetTicket(qrcode);
        }



        //Round operations
        [HttpGet()]
        public bool StartRound(string uid)
        {
            return _orderService.StartRound(uid);
        }


        [HttpGet()]
        public bool CancelRound(string uid)
        {
            return _orderService.CancelRound(uid);
        }


        [HttpGet()]
        public bool FinishRound(string uid)
        {
            _orderService.FinishRound(uid);
        }



        //Round Info
        [HttpGet()]
        public RoundDetail GetRoundDetail(string uid)
        {
            return _orderService.GetRoundDetail(uid);
        }

        [HttpGet()]
        public List<TripRound>? GetRoundList(DateOnly date)
        {
            return _orderService.GetRoundList(date);
        }





        [HttpGet()]
        public void AddPrintLog(string orderId)
        {
            int _orderId = 0;
            int.TryParse(orderId, out _orderId);

            _orderService.AddPrintLog(_orderId);
        }

        [HttpGet()]
        public List<Vorder> GetOrders()
        {
            return _orderService.GetOrders();
        }

        [HttpGet()]
        public VOrderInfo? GetOrder(int id)
        {
            return _orderService.GetOrder(id);
        }


        [HttpGet()]
        public OrderRefund GetOrderRefund(int id)
        {
            return _orderService.GetOrderRefund(id);
        }



        [HttpGet()]
        public int GetOrderId(string qr)
        {
            return _orderService.GetOrderId(qr);
        }

        [HttpGet()]
        public List<VorderRow>? GetOrderRows(int id)
        {
            return _orderService.GetOrderRows(id);
        }

        [HttpGet()]
        public List<OrderBasket>? GetOrderBasket(int id)
        {
            return _orderService.GetOrderBasket(id);
        }


        [HttpGet()]
        public bool? CancelOrder(int id, int employeeId)
        {
            return _orderService.CancelOrder(id, employeeId);
        }

        [HttpGet()]
        public AppResult OrderRowReusable(int id, int employeeId)
        {
            return _orderService.OrderRowReusable(id, employeeId);
        }


        [HttpGet()]
        public AppResult OrderRefundCheck(int id)
        {
            return _orderService.OrderRefundCheck(id);
        }

        [HttpPost()]
        public AppResult AddOrderRefund([FromBody] AddOrderRefund refund)
        {
            return _orderService.AddOrderRefund(refund.Id, refund.EmployeeId, refund.RefundTypeId, refund.Description);
        }



    }
}
