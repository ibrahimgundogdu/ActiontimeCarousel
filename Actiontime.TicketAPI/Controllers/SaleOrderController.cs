using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Actiontime.TicketAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SaleOrderController : ControllerBase
    {
        SaleOrderService _orderService;
        CloudService _cloudService;
        private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
        private readonly IDbContextFactory<ApplicationCloudDbContext> _cdbFactory;

        public SaleOrderController(IDbContextFactory<ApplicationDbContext> dbFactory, IDbContextFactory<ApplicationCloudDbContext> cdbFactory, CloudService cloudService)
        {

            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _cdbFactory = cdbFactory ?? throw new ArgumentNullException(nameof(cdbFactory));
            _cloudService = cloudService ?? throw new ArgumentNullException(nameof(cloudService));

            _orderService = new SaleOrderService(dbFactory, cdbFactory, _cloudService);

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
