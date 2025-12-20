using Actiontime.Data.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services.Interfaces
{
    public interface ISaleOrderService
    {
        bool AddBasket(Basket? item);
        AddOrderResult AddOrder(string token, int paymethodId, AppBasketItem[] items);
        TicketReceipt GetReceipt(int orderId);
        void AddPrintLog(int orderId);
        List<Vorder> GetOrders();
        List<VorderRow>? GetOrderRows(int orderId);
        List<OrderBasket>? GetOrderBasket(int id);
        VOrderInfo? GetOrder(int orderId);
        OrderRefund GetOrderRefund(int orderId);
        AppResult OrderRefundCheck(int orderId);
        bool? CancelOrder(int orderId, int employeeId);
        AppResult OrderRowReusable(int rowId, int employeeId);
        bool? DeleteOrder(int orderId, int employeeId);
        int GetOrderId(string ticket);
        void CheckOrderAction(int orderId, int employeeId);
        void CheckLocationPosTicketSale(long orderId);
        AppResult AddOrderRefund(int OrderId, int employeeId, int refundTypeId, string Description);

    }
}
