using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Actiontime.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Actiontime.Services
{
    public class SaleOrderService : ISaleOrderService
    {

        private readonly ICloudService _cloudService;
        private ApplicationDbContext _db;
        private ApplicationCloudDbContext _cdb;
        private readonly IServiceScopeFactory _scopeFactory;

        public SaleOrderService(ApplicationDbContext db, ApplicationCloudDbContext cdb, ICloudService cloudService, IServiceScopeFactory scopeFactory)
        {

            _db = db;
            _cdb = cdb;
            _cloudService = cloudService;
            _scopeFactory = scopeFactory;
        }

        public bool AddBasket(Basket? item)
        {

            bool result = false;

            if (item != null)
            {

                try
                {
                    var isExistsItem = _db.Baskets.FirstOrDefault(x => x.Token == item.Token && x.AppBasketId == item.AppBasketId && x.LocationId == item.LocationId && x.EmployeeId == item.EmployeeId);

                    if (isExistsItem != null)
                    {
                        _db.Baskets.Remove(isExistsItem);
                        _db.SaveChanges();
                    }

                    Basket basket = new Basket();

                    basket.AppBasketId = (int)item.AppBasketId;
                    basket.LocationId = item.LocationId;
                    basket.EmployeeId = item.EmployeeId;
                    basket.PriceCategoryId = item.PriceCategoryId;
                    basket.PriceId = item.PriceId;
                    basket.ProductId = item.ProductId;
                    basket.Date = DateTime.Now;
                    basket.Quantity = item.Quantity;
                    basket.Unit = item.Unit;
                    basket.Price = item.Price;
                    basket.TaxRate = item.TaxRate;
                    basket.Tax = item.Tax;
                    basket.Total = item.Total;
                    basket.Currency = item.Currency;
                    basket.PromotionId = item.PromotionId;
                    basket.IsPromotion = item.IsPromotion;
                    basket.RecordEmployeeId = item.RecordEmployeeId;
                    basket.RecordDate = DateTime.Now;
                    basket.MasterPrice = item.MasterPrice;
                    basket.PromoPrice = item.PromoPrice;
                    basket.TotalPrice = item.TotalPrice;
                    basket.Token = item.Token;
                    basket.Discount = item.Discount;

                    _db.Baskets.Add(basket);
                    _db.SaveChanges();

                    result = true;

                }
                catch (Exception ex)
                {

                }
            }
            return result;
        }

        public AddOrderResult AddOrder(string token, int paymethodId, AppBasketItem[] items)
        {
            AddOrderResult result = new AddOrderResult()
            {
                id = 0,
                orderNumber = string.Empty,
                currency = string.Empty,
                total = 0,
                uid = string.Empty,
                printCount = 0
            };

            Guid _token = Guid.Parse(token);

            if (!string.IsNullOrEmpty(token))
            {

                if (paymethodId > 0)
                {
                    if (items != null && items.Length > 0)
                    {
                        try
                        {
                            var location = _db.OurLocations.FirstOrDefault();

                            // bu tokene ait baskette bişiler varsa sil
                            var basketexists = _db.Baskets.Where(x => x.Token == _token).ToList();
                            _db.Baskets.RemoveRange(basketexists);
                            _db.SaveChanges();

                            //bu tokene ait basketi ekle
                            List<Basket> basketItems = new List<Basket>();
                            int employeeID = 6070;

                            foreach (var item in items)
                            {
                                basketItems.Add(new Basket()
                                {
                                    Currency = item.currency ?? "USD",
                                    Date = DateTime.Now,
                                    Discount = item.discount ?? 0,
                                    EmployeeId = item.employeeId ?? 0,
                                    IsPromotion = item.isPromotion,
                                    LocationId = item.locationId ?? 0,
                                    MasterPrice = item.masterPrice ?? 0,
                                    Price = item.price ?? 0,
                                    PriceCategoryId = (short)(item.priceCategoryId ?? 0),
                                    PriceId = (short)(item.priceId ?? 0),
                                    ProductId = item.productId ?? 0,
                                    PromoPrice = item.promoPrice ?? 0,
                                    PromotionId = item.promotionId,
                                    Quantity = item.quantity ?? 0,
                                    RecordDate = DateTime.Now,
                                    RecordEmployeeId = item.recordEmployeeId ?? 0,
                                    Tax = item.tax ?? 0,
                                    TaxRate = item.taxRate ?? 0,
                                    Total = item.total ?? 0,
                                    TotalPrice = item.totalPrice ?? 0,
                                    Unit = item.unit ?? 0,
                                    AppBasketId = item.id,
                                    Token = _token
                                });

                                employeeID = item.recordEmployeeId ?? 6070;
                            }

                            _db.Baskets.AddRange(basketItems);
                            _db.SaveChanges();


                            int? orderID = null;


                            var order = _db.Orders.FirstOrDefault(x => x.Token == _token && x.LocationId == location.Id);

                            if (order == null)
                            {
                                var parameterReturn = new SqlParameter
                                {
                                    ParameterName = "@DocumentNumber",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Direction = System.Data.ParameterDirection.Output,
                                    Size = 50,
                                };

                                var sqldoc = "EXEC GetOrderNumber @DocumentNumber OUT";
                                _db.Database.ExecuteSqlRaw(sqldoc, parameterReturn);

                                var OrderNumber = $"S{location.Id}" + (string)parameterReturn.Value;

                                var parameterOrderId = new SqlParameter
                                {
                                    ParameterName = "@OrderID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Direction = System.Data.ParameterDirection.Output
                                };

                                var parameterToken = new SqlParameter
                                {
                                    ParameterName = "@Token",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Size = 50,
                                    Value = token,
                                };
                                var parameterOrderNumber = new SqlParameter
                                {
                                    ParameterName = "@OrderNumber",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Size = 50,
                                    Value = OrderNumber,
                                };

                                var parameterPaymethodID = new SqlParameter
                                {
                                    ParameterName = "@PaymethodID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = paymethodId

                                };


                                var sqlorder = "EXEC AddOrder @Token, @OrderNumber, @PaymethodID, @OrderID OUT";
                                _db.Database.ExecuteSqlRaw(sqlorder, parameterToken, parameterOrderNumber, parameterPaymethodID, parameterOrderId);
                                orderID = (int)parameterOrderId.Value;

                                if (orderID > 0)
                                {
                                    order = _db.Orders.FirstOrDefault(x => x.Id == orderID);

                                    if (order != null)
                                    {

                                        SyncProcess process = new SyncProcess()
                                        {
                                            DateCreate = DateTime.Now,
                                            Entity = "Order",
                                            Process = 1,
                                            EntityId = order.Id,
                                            EntityUid = order.Uid
                                        };

                                        _db.SyncProcesses.Add(process);

                                        order.SyncDate = DateTime.Now;
                                        _db.SaveChanges();

                                        CheckOrderAction(order.Id, employeeID);
                                        CheckLocationPosTicketSale(order.Id);



                                        // create a new scope for background work to avoid using a disposed scoped provider
                                        Task.Run(() =>
                                        {
                                            using var scope = _scopeFactory.CreateScope();
                                            var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                                            // re-load the SyncProcess from the worker scope to avoid using an entity tracked by the request scope
                                            var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                            var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                                            if (persistedProcess != null)
                                            {
                                                scopedCloud.AddCloudProcess(persistedProcess);
                                            }
                                        });



                                        result.orderNumber = order.OrderNumber;
                                        result.total = order.TotalAmount ?? 0;
                                        result.currency = order.Currency ?? "USD";
                                        result.uid = order.Uid.ToString();
                                        result.id = order.Id;
                                        result.printCount = order.PrintCount ?? 0;



                                    }
                                }

                            }
                            else
                            {
                                result.orderNumber = order.OrderNumber;
                                result.total = order.TotalAmount ?? 0;
                                result.currency = order.Currency ?? "USD";
                                result.uid = order.Uid.ToString();
                                result.id = order.Id;
                                result.printCount = order.PrintCount ?? 0;

                                CheckOrderAction(order.Id, employeeID);
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                }

            }

            return result;
        }

        //GetReceipt
        public TicketReceipt GetReceipt(int orderId)
        {

            TicketReceipt receipt = new TicketReceipt();

            if (orderId > 0)
            {


                try
                {
                    var location = _db.OurLocations.FirstOrDefault();
                    var order = _db.Orders.FirstOrDefault(x => x.Id == orderId);

                    if (order != null)
                    {
                        var ourcompany = _db.OurCompanies.FirstOrDefault(x => x.Id == order.OurCompanyId);
                        var orderRows = _db.OrderRows.Where(x => x.OrderId == orderId).ToList();
                        var payments = _db.OrderPosPayments.Where(x => x.OrderId == orderId).ToList();



                        var subtotal = orderRows.Sum(x => x.Amount) ?? 0;
                        var total = orderRows.Sum(x => x.Total) ?? 0;
                        var discount = orderRows.Sum(x => x.Discount) ?? 0;
                        var charge = payments.Sum(x => x.PaymentAmount);

                        List<SaleRow> rows = new List<SaleRow>();
                        List<Ticket> tickets = new List<Ticket>();

                        receipt.FooterMessage = "Thank you for visiting";
                        receipt.FooterHeader = "We appreciate your business";
                        receipt.CompanyName = ourcompany?.CompanyName ?? "Action Time";
                        receipt.Address = location.Address ?? "USA";
                        receipt.PhoneNumber = location.PhoneNumber ?? "(214) 546-1920";
                        receipt.Hour = order.RecordDate?.ToString("hh:mm tt", new System.Globalization.CultureInfo("en-US")) ?? "00:00 AM";
                        receipt.Date = order.RecordDate?.ToString("M-dd-yyyy") ?? DateTime.Now.ToString("M-dd-yyyy");
                        receipt.SaleID = $"MC NO.{order.Id}" ?? "MC NO.0000";
                        receipt.Counter = order.ReceiptNumber;
                        receipt.Total = "$" + total.ToString("N2");
                        receipt.SubTotal = "$" + subtotal.ToString("N2");
                        receipt.Tax = "$" + (total - subtotal).ToString("N2");
                        receipt.Discount = "$" + discount.ToString("N2");
                        receipt.TotalTax = "$" + (total - subtotal).ToString("N2");
                        receipt.Charge = "$" + charge.ToString("N2");
                        receipt.Id = order.Id.ToString();
                        receipt.EmployeeId = order.EmployeeId.ToString();
                        receipt.PrintCount = order.PrintCount.ToString();

                        foreach (var item in orderRows)
                        {
                            rows.Add(new SaleRow()
                            {
                                ItemName = location.LocationTypeName + " " + $"{item.Duration}M",
                                Price = "$" + (item.Quantity * item.Price).ToString("N2")
                            });
                        }
                        receipt.Rows = rows;

                        foreach (var item in orderRows)
                        {
                            tickets.Add(new Ticket()
                            {
                                TicketNumber = $"{item.Id}-{item.LocationId}-{item.TicketNumber}",
                                TicketName = $"{item.Unit} Min"

                            });
                        }

                        receipt.Tickets = tickets;
                    }




                }
                catch (Exception ex)
                {

                }
            }

            return receipt;
        }

        public void AddPrintLog(int orderId)
        {

            try
            {
                var order = _db.Orders.FirstOrDefault(x => x.Id == orderId);

                if (order != null)
                {
                    order.PrintCount = order.PrintCount + 1;
                    _db.SaveChanges();
                }

            }
            catch (Exception ex)
            {

            }
        }

        public List<Vorder> GetOrders()
        {
            var date = _db.OurLocations.FirstOrDefault()?.LocalDate;

            return _db.Vorders.Where(x => x.Date >= date).ToList();
        }

        public List<VorderRow>? GetOrderRows(int orderId)
        {
            return _db.VorderRows.Where(x => x.OrderId == orderId).ToList();
        }

        public List<OrderBasket>? GetOrderBasket(int orderId)
        {
            return _db.OrderBaskets.Where(x => x.OrderId == orderId).ToList();
        }

        public VOrderInfo? GetOrder(int orderId)
        {
            VOrderInfo vOrderInfo = new VOrderInfo();

            var _order = _db.Vorders.FirstOrDefault(x => x.Id == orderId);

            vOrderInfo.Date = _order.Date;
            vOrderInfo.RecordDate = _order.RecordDate;
            vOrderInfo.OrderNumber = _order.OrderNumber;
            vOrderInfo.ReceiptNumber = _order.ReceiptNumber;
            vOrderInfo.PayMethodName = _order.PayMethodName;
            vOrderInfo.EmployeeFullName = _order.EmployeeFullName;
            vOrderInfo.EmployeeId = _order.EmployeeId;
            vOrderInfo.Id = _order.Id;
            vOrderInfo.LocationId = _order.LocationId;
            vOrderInfo.Uid = _order.Uid;
            vOrderInfo.TotalAmount = _order.TotalAmount;
            vOrderInfo.TicketCount = _order.TicketCount;
            vOrderInfo.Sign = _order.Sign;
            vOrderInfo.SaleStatusName = _order.SaleStatusName;
            vOrderInfo.PrintCount = _order.PrintCount;
            vOrderInfo.PaymentType = _order.PaymentType;
            vOrderInfo.PaymentAmount = _order.PaymentAmount;

            vOrderInfo.OrderRows = _db.VorderRows.Where(x => x.OrderId == orderId).ToList();
            vOrderInfo.OrderItems = _db.VorderItems.Where(x => x.OrderId == orderId).ToList();


            return vOrderInfo;
        }

        public OrderRefund GetOrderRefund(int orderId)
        {
            OrderRefund model = new OrderRefund();

            var order = _db.Vorders.FirstOrDefault(x => x.Id == orderId);

            if (order != null)
            {
                model.Date = order.Date;
                model.RecordDate = order.RecordDate ?? DateTime.MinValue;
                model.OrderNumber = order.OrderNumber;
                model.PayMethodName = order.PayMethodName ?? "";
                model.EmployeeFullName = order.EmployeeFullName ?? "";
                model.Id = order.Id;
                model.Uid = order.Uid;
                model.TotalAmount = order.TotalAmount ?? 0;
                model.TicketCount = order.TicketCount ?? 0;
                model.Sign = order.Sign ?? "$";
                model.SaleStatusName = order.SaleStatusName ?? "";
                model.SaleStatusId = order.OrderStatusId;
                model.PrintCount = order.PrintCount ?? 0;
                model.PaymentAmount = order.PaymentAmount ?? 0;

                var refund = _db.OrderPosRefunds.FirstOrDefault(x => x.OrderId == orderId);

                if (refund != null)
                {
                    model.RefundAmount = refund.RefundAmount;
                    model.RefundMethodName = refund.RefundType == 1 ? "Cash" : refund.RefundType == 2 ? "Credit" : "";
                    model.RefundDescription = refund.Description;
                }
                else
                {
                    model.RefundAmount = 0;
                    model.RefundMethodName = string.Empty;
                    model.RefundDescription = string.Empty;
                }


                if (order.OrderStatusId == 1) // sale completed
                {


                    model.RefundAmount = 0;
                    model.RefundMethodName = string.Empty;
                    model.RefundDescription = string.Empty;

                    var orderRows = _db.OrderRows.Where(x => x.OrderId == orderId).ToList();
                    if (orderRows != null)
                    {
                        if (orderRows.Any(x => x.RowStatusId == 2))
                        {
                            var confirms = _db.TripConfirms.Where(x => x.SaleOrderId == orderId).ToList();
                            if (confirms != null && confirms.Count > 0)
                            {
                                List<long> confirmIds = confirms.Select(x => x.Id).ToList();

                                var trips = _db.Trips.Where(x => confirmIds.Contains(x.ConfirmId.Value)).ToList();

                                if (trips != null && trips.Count > 0)
                                {
                                    if (trips.Any(x => x.TripDurationSecond > 30))
                                    {
                                        model.Success = false;
                                        model.Message = "Not Refundable";
                                        model.Description = "There is Used Ticket";
                                    }
                                    else if (trips.Any(x => x.TripDurationSecond <= 30))
                                    {
                                        model.Success = false;
                                        model.Message = "Not Refundable";
                                        model.Description = "There is reusable ticket!";
                                    }
                                    else if (trips.Any(x => x.TripDurationSecond == null))
                                    {
                                        model.Success = false;
                                        model.Message = "Not Refundable";
                                        model.Description = "There is Ticket in Use";
                                    }
                                }
                                else
                                {
                                    model.Success = true;
                                    model.Message = "Refundable";
                                    model.Description = "There are Not Used Tickets";
                                }
                            }
                            else
                            {
                                model.Success = true;
                                model.Message = "Refundable";
                                model.Description = "There are Not Used Tickets";
                            }
                        }
                        else
                        {
                            model.Success = false;
                            model.Message = "Not Refundable";
                            model.Description = "There are Used Tickets";
                        }
                    }
                    else
                    {
                        model.Success = false;
                        model.Message = "Not Refundable";
                        model.Description = "There are No Tickets";
                    }



                }
                else
                {
                    model.Success = false;
                    model.Message = "Not Refundable";
                    model.Description = "Order Status is no compatiple";
                }

            }
            else
            {
                model.Success = false;
                model.Message = "Not Refundable";
                model.Description = "Order ID is wrong";
            }

            return model;
        }

        public AppResult OrderRefundCheck(int orderId)
        {
            AppResult result = new AppResult() { Success = true, Message = "Refundable", Description = string.Empty };

            var order = _db.Orders.FirstOrDefault(x => x.Id == orderId);
            if (order != null)
            {
                if (order.OrderStatusId == 1)
                {

                    var orderRows = _db.OrderRows.Where(x => x.OrderId == orderId).ToList();
                    if (orderRows != null)
                    {
                        if (orderRows.Any(x => x.RowStatusId == 2))
                        {
                            var confirms = _db.TripConfirms.Where(x => x.SaleOrderId == orderId).ToList();
                            if (confirms != null && confirms.Count > 0)
                            {
                                List<long> confirmIds = confirms.Select(x => x.Id).ToList();

                                var trips = _db.Trips.Where(x => confirmIds.Contains(x.ConfirmId.Value)).ToList();

                                if (trips != null && trips.Count > 0)
                                {
                                    if (trips.Any(x => x.TripDurationSecond > 30))
                                    {
                                        result.Success = false;
                                        result.Message = "Not Refundable";
                                        result.Description = "There are Used Tickets";
                                    }
                                    else if (trips.Any(x => x.TripDurationSecond == null))
                                    {
                                        result.Success = false;
                                        result.Message = "Not Refundable";
                                        result.Description = "There are Tickets in Use";
                                    }
                                }
                            }
                        }
                        else
                        {
                            result.Success = false;
                            result.Message = "Not Refundable";
                            result.Description = "There are Used Tickets";
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Not Refundable";
                        result.Description = "There are No Tickets";
                    }

                }
                else
                {
                    result.Success = false;
                    result.Message = "Not Refundable";
                    result.Description = "Order Status is no compatiple";
                }

            }
            else
            {
                result.Success = false;
                result.Message = "Not Refundable";
                result.Description = "Order ID is wrong";
            }

            return result;
        }

        public bool? CancelOrder(int orderId, int employeeId)
        {
            bool isCancelled = false;

            var _order = _db.Orders.FirstOrDefault(x => x.Id == orderId);

            if (_order == null)
            {
                _order.OrderStatusId = 2;
                _order.UpdateDate = DateTime.Now;
                _order.UpdateEmployeeId = employeeId;

                _db.SaveChanges();

                isCancelled = true;
            }

            var rows = _db.OrderRows.Where(x => x.OrderId == orderId).ToList();
            foreach (var item in rows)
            {
                item.RowStatusId = 4;
                item.UpdateDate = DateTime.Now;
                item.UpdateEmployeeId = employeeId;

                _db.SaveChanges();
            }

            CheckOrderAction(orderId, employeeId);

            return isCancelled;
        }

        public AppResult OrderRowReusable(int rowId, int employeeId)
        {
            AppResult result = new AppResult() { Success = false, Message = "Not Successful", Description = string.Empty };

            var location = _db.OurLocations.FirstOrDefault();

            var orderRow = _db.OrderRows.FirstOrDefault(x => x.Id == rowId);

            if (orderRow != null)
            {
                var confirm = _db.TripConfirms.FirstOrDefault(x => x.SaleOrderRowId == orderRow.Id && x.SaleOrderId == orderRow.OrderId);

                if (confirm != null)
                {
                    var trip = _db.Trips.FirstOrDefault(x=> x.ConfirmId == confirm.Id);

                    if (trip != null && trip.TripDurationSecond != null && trip.TripDurationSecond <= 30)
                    {
                        TripHistory hist = new TripHistory();

                        hist.TripId = trip.Id;
                        hist.ConfirmId = confirm.Id;
                        hist.LocationId = confirm.LocationId;
                        hist.EmployeeId = trip.EmployeeId;
                        hist.TicketNumber = confirm.TicketNumber;
                        hist.ReaderSerialNumber = confirm.ReaderSerialNumber;
                        hist.PartId = confirm.LocationPartId;
                        hist.TripDate = trip.TripDate;
                        hist.TripStart = trip.TripStart;
                        hist.TripCancel = trip.TripCancel;
                        hist.TripEnd = trip.TripEnd;
                        hist.UnitDuration = confirm.UnitDuration;
                        hist.TripDuration = trip.TripDuration;
                        hist.TripDurationSecond = trip.TripDurationSecond;
                        hist.RecordEmployeeId = employeeId;
                        hist.RecordDate = location?.LocalDateTime;
                        hist.Uid = trip.Uid;
                        hist.ConfirmNumber = confirm.ConfirmNumber;
                        hist.SaleOrderId = confirm.SaleOrderId;
                        hist.SaleOrderRowId = confirm.SaleOrderRowId;
                        hist.ConfirmTime = confirm.ConfirmTime;
                        hist.IsApproved = confirm.IsApproved;
                        hist.CreaterId = employeeId;
                        hist.CreateDate = location?.LocalDateTime;

                        _db.TripHistories.Add(hist);
                        _db.SaveChanges();

                        result.Success = true;
                        result.Message = "Reusable Succesful";

                        _db.TripConfirms.Remove(confirm);
                        _db.SaveChanges();

                        _db.Trips.Remove(trip);
                        _db.SaveChanges();

                        orderRow.RowStatusId = 2;
                        _db.SaveChanges();

                    }
                    else
                    {
                        result.Message = "Trip is not compatible";
                    }
                }
                else
                {
                    result.Message = "Confirm not found";
                }
               
            }
            else
            {
                result.Message = "Ticket not found";
            }

           

            return result;
        }

        public bool? DeleteOrder(int orderId, int employeeId)
        {
            bool isDeleted = false;

            var _order = _db.Orders.FirstOrDefault(x => x.Id == orderId);

            if (_order == null)
            {
                _order.OrderStatusId = 4;
                _order.UpdateDate = DateTime.Now;
                _order.UpdateEmployeeId = employeeId;

                _db.SaveChanges();

                isDeleted = true;
            }

            var rows = _db.OrderRows.Where(x => x.OrderId == orderId).ToList();
            foreach (var item in rows)
            {
                item.RowStatusId = 4;
                item.UpdateDate = DateTime.Now;
                item.UpdateEmployeeId = employeeId;

                _db.SaveChanges();
            }

            CheckOrderAction(orderId, employeeId);

            return isDeleted;
        }

        public int GetOrderId(string ticket)
        {
            int orderId = -1;

            int rowId = 0;
            int locationId = 0;
            string ticketNumber = string.Empty;


            var qrParts = ticket.Split("-");
            if (qrParts.Length == 7)
            {
                _ = int.TryParse(qrParts[0], out rowId);
                _ = int.TryParse(qrParts[1], out locationId);

                ticketNumber = $"{qrParts[2]}-{qrParts[3]}-{qrParts[4]}-{qrParts[5]}-{qrParts[6]}";
                //ticketNumber = ticket.Substring(qrParts[0].Length + 1 + qrParts[1].Length + 1);
            }
            var orderRow = _db.VorderRows.FirstOrDefault(x => x.Id == rowId && x.LocationId == locationId && x.TicketNumber == ticketNumber);
            if (orderRow != null)
            {
                return orderRow.OrderId;
            }

            return orderId;
        }

        public void CheckOrderAction(int orderId, int employeeId)
        {
            var _order = _db.Orders.FirstOrDefault(x => x.Id == orderId);

            if (_order != null)
            {
                var parameterOrderId = new SqlParameter
                {
                    ParameterName = "@OrderID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Value = orderId,
                };

                var parameterEmployeeID = new SqlParameter
                {
                    ParameterName = "@EmployeeID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Value = employeeId
                };

                var sqlorder = "EXEC AddOrderAction @OrderID, @EmployeeID";
                _db.Database.ExecuteSqlRaw(sqlorder, parameterOrderId, parameterEmployeeID);

            }

        }

        public void CheckLocationPosTicketSale(long orderId)
        {
            var parameterSaleID = new SqlParameter
            {
                ParameterName = "@SaleID",
                SqlDbType = System.Data.SqlDbType.Int,
                Value = orderId,
            };

            var sqlorder = "EXEC CheckLocationPosTicketSale @SaleID";
            _cdb.Database.ExecuteSqlRaw(sqlorder, parameterSaleID);

        }

        public AppResult AddOrderRefund(int OrderId, int employeeId, int refundTypeId, string Description)
        {
            AppResult result = new AppResult() { Success = true, Message = "Refunded", Description = "Refund Succeded" };

            var order = _db.Orders.FirstOrDefault(x => x.Id == OrderId);
            var orderrows = _db.OrderRows.Where(x => x.OrderId == OrderId).ToList();
            var payment = _db.OrderPosPayments.FirstOrDefault(x => x.OrderId == OrderId);
            var refund = _db.OrderPosRefunds.FirstOrDefault(x => x.OrderId == OrderId);

            var location = _db.OurLocations.FirstOrDefault();
            var processDate = location.LocalDate.Value;
            var processDateTime = location.LocalDateTime.Value;



            if (order != null)
            {

                // Refund a uygun olup olmadığı tekrar kontrol edilir.

                var isRefundable = OrderRefundCheck(OrderId);

                if (isRefundable != null && isRefundable.Success == true)
                {

                    order.OrderStatusId = 3;
                    order.UpdateDate = processDateTime;
                    order.UpdateEmployeeId = employeeId;

                    _db.SaveChanges();


                    if (orderrows != null && orderrows.Count > 0)
                    {
                        foreach (var item in orderrows)
                        {
                            item.RowStatusId = 5;
                            item.UpdateDate = processDateTime;
                            item.UpdateEmployeeId = employeeId;

                        }
                        _db.SaveChanges();
                    }

                    if (payment != null)
                    {
                        if (refund == null)
                        {
                            refund = new OrderPosRefund();

                            refund.OrderId = order.Id;
                            refund.RefundType = refundTypeId;
                            refund.RefundAmount = payment.PaymentAmount;
                            refund.Currency = payment.Currency;
                            refund.RefoundDate = processDateTime;
                            refund.DocumentNumber = order.OrderNumber;
                            refund.FromPosTerminal = false;
                            refund.RecordDate = processDateTime;
                            refund.Description = Description;
                            refund.RecordEmployeeId = employeeId;

                            _db.OrderPosRefunds.Add(refund);
                            _db.SaveChanges();
                        }
                        else
                        {
                            refund.OrderId = order.Id;
                            refund.RefundType = refundTypeId;
                            refund.RefundAmount = payment.PaymentAmount;
                            refund.Currency = payment.Currency;
                            refund.DocumentNumber = order.OrderNumber;
                            refund.FromPosTerminal = false;
                            refund.RecordDate = processDateTime;
                            refund.Description = Description;
                            refund.RecordEmployeeId = employeeId;

                            _db.SaveChanges();
                        }
                    }

                    if (refundTypeId == 1)
                    {
                        var cashrefund = _db.CashActions.FirstOrDefault(x => x.OrderId == order.Id && x.CashActionTypeId == 28);
                        var cash = _db.Cashes.FirstOrDefault(x => x.CashTypeId == 1 && x.IsMaster == true);

                        if (cashrefund != null)
                        {
                            _db.CashActions.Remove(cashrefund);
                            _db.SaveChanges(true);
                        }

                        cashrefund = new Data.Entities.CashAction();

                        cashrefund.CashId = cash.Id;
                        cashrefund.LocationId = cash.LocationId;
                        cashrefund.CashActionTypeId = 28;
                        cashrefund.ActionDate = DateOnly.FromDateTime(processDateTime);
                        cashrefund.OrderId = order.Id;
                        cashrefund.ProcessId = refund.Id;
                        cashrefund.Collection = 0;
                        cashrefund.Payment = refund.RefundAmount;
                        cashrefund.Currency = refund.Currency;
                        cashrefund.RecordEmployeeId = employeeId;
                        cashrefund.RecordDate = processDateTime;
                        cashrefund.ProcessUid = refund.Uid;

                        _db.CashActions.Add(cashrefund);
                        _db.SaveChanges(true);
                    }


                    if (refundTypeId == 2)
                    {
                        var bankrefund = _db.BankActions.FirstOrDefault(x => x.OrderId == order.Id && x.BankActionTypeId == 3);
                        var bank = _db.Banks.FirstOrDefault(x => x.IsActive == true);

                        if (bankrefund != null)
                        {
                            _db.BankActions.Remove(bankrefund);
                            _db.SaveChanges(true);
                        }

                        bankrefund = new BankAction();

                        bankrefund.BankId = bank.Id;
                        bankrefund.LocationId = order.LocationId;
                        bankrefund.BankActionTypeId = 3;
                        bankrefund.ActionDate = DateOnly.FromDateTime(DateTime.Now);
                        bankrefund.OrderId = order.Id;
                        bankrefund.ProcessId = refund.Id;
                        bankrefund.Collection = 0;
                        bankrefund.Payment = refund.RefundAmount;
                        bankrefund.Currency = refund.Currency;
                        bankrefund.RecordEmployeeId = employeeId;
                        bankrefund.RecordDate = DateTime.Now;
                        bankrefund.ProcessUid = refund.Uid;

                        _db.BankActions.Add(bankrefund);
                        _db.SaveChanges(true);
                    }


                    result = new AppResult() { Success = isRefundable.Success, Message = isRefundable.Message, Description = isRefundable.Description };

                    SyncProcess process = new SyncProcess()
                    {
                        DateCreate = processDateTime,
                        Entity = "OrderRefund",
                        Process = 3,
                        EntityId = order.Id,
                        EntityUid = order.Uid
                    };

                    _db.SyncProcesses.Add(process);

                    order.SyncDate = processDateTime;
                    _db.SaveChanges();

                    //Task task = Task.Run(() => _cloudService.AddCloudProcess(process));

                    Task.Run(() =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var scopedCloud = scope.ServiceProvider.GetRequiredService<ICloudService>();
                        var workerDb = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var persistedProcess = workerDb.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (persistedProcess != null)
                        {
                            scopedCloud.AddCloudProcess(persistedProcess);
                        }
                    });

                }
            }

            return result;
        }




    }
}
