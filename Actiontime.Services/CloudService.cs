using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.DataCloud.Entities;
using Actiontime.Models;
using Actiontime.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Actiontime.Services
{
    public class CloudService: ICloudService
    {
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        private readonly IServiceProvider _sp;
        

        public CloudService(ApplicationDbContext db, ApplicationCloudDbContext cdb, IServiceProvider sp)
        {
            _db = db;
            _cdb = cdb;
            _sp = sp;
        }

        public void AddCloudProcess(SyncProcess process)
        {

            if (process.Entity == "Order" && process.Process == 1)
            {
                var localOrder = _db.Orders.FirstOrDefault(x => x.Id == process.EntityId);

                if (localOrder != null)
                {
                    // ISaleOrderService artık constructor ile gelmiyor; gerektiğinde resolve et
                    var orderService = _sp.GetService<ISaleOrderService>();
                    orderService?.CheckOrderAction(localOrder.Id, localOrder.EmployeeId ?? 6070);
                }

                var localOrderRows = _db.OrderRows.Where(x => x.OrderId == process.EntityId).ToList();
                var localPosPayment = _db.OrderPosPayments.FirstOrDefault(x => x.OrderId == process.EntityId);

                var cloudOrder = _cdb.TicketSales.FirstOrDefault(x => x.Uid == process.EntityUid && x.LocalOrderId == process.EntityId);

                try
                {
                    if (cloudOrder == null)
                    {

                        TicketSale ticketSale = new TicketSale();

                        ticketSale.StatusId = localOrder.OrderStatusId;
                        ticketSale.OrderNumber = localOrder.OrderNumber;
                        ticketSale.SaleTypeId = localOrder.OrderTypeId;
                        ticketSale.Date = localOrder.Date;
                        ticketSale.OurCompanyId = localOrder.OurCompanyId;
                        ticketSale.LocationId = localOrder.LocationId;
                        ticketSale.EmployeeId = localOrder.EmployeeId;
                        ticketSale.Description = localOrder.Description;
                        ticketSale.SaleChannelD = localOrder.SaleChannelD;
                        ticketSale.PriceCategoryId = localOrder.PriceCategoryId;
                        ticketSale.PaymethodId = localPosPayment?.PaymentType;
                        ticketSale.Amount = localOrder.TotalAmount;
                        ticketSale.Currency = localOrder.Currency;
                        ticketSale.PosStatusId = localOrder.IsPaymentCompleted == true ? 3 : 0;
                        ticketSale.EnvironmentId = 7;
                        ticketSale.Uid = localOrder.Uid;
                        ticketSale.LocalOrderId = localOrder.Id;
                        ticketSale.RecordEmployeeId = localOrder.EmployeeId;
                        ticketSale.RecordDate = localOrder.RecordDate;
                        ticketSale.IsSendPosTerminal = localOrder.SendPaymentTerminal;
                        ticketSale.IsFinancialization = false;
                        ticketSale.IsActive = localOrder.IsActive;

                        _cdb.TicketSales.Add(ticketSale);
                        _cdb.SaveChanges();

                        //ticketsalerows eklenir

                        foreach (var row in localOrderRows)
                        {
                            TicketSaleRow saleRow = new TicketSaleRow();

                            saleRow.SaleId = ticketSale.Id;
                            saleRow.StatusId = row.RowStatusId;
                            saleRow.Date = row.Date;
                            saleRow.LocationId = row.LocationId;
                            saleRow.EmployeeId = row.EmployeeId;
                            saleRow.PaymethodId = row.PaymethodId;
                            saleRow.UseImmediately = true;
                            saleRow.PriceCategoryId = row.PriceCategoryId;
                            saleRow.TicketTypeId = row.TicketTypeId;
                            saleRow.TicketNumber = $"{row.Id}-{row.LocationId}-{row.TicketNumber}";
                            saleRow.Quantity = row.Quantity;
                            saleRow.Unit = row.Unit;
                            saleRow.ExtraUnit = row.ExtraUnit;
                            saleRow.PriceId = row.PriceId;
                            saleRow.Price = row.Price;
                            saleRow.Discount = row.Discount;
                            saleRow.ExtraPrice = row.ExtraPrice;
                            saleRow.PrePaid = row.PrePaid;
                            saleRow.Currency = row.Currency;
                            saleRow.TaxRate = row.TaxRate;
                            saleRow.PromotionId = row.PromotionId;
                            saleRow.IsPromotion = row.IsPromotion;
                            saleRow.IsExchangable = true;
                            saleRow.DeviceId = row.DeviceId;
                            saleRow.RecordEmployeeId = row.RecordEmployeeId;
                            saleRow.RecordDate = row.RecordDate;
                            saleRow.Uid = row.Uid;
                            saleRow.LocalRowId = row.Id;
                            saleRow.ProductId = row.ProductId;
                            saleRow.MasterCredit = 0;
                            saleRow.PromoCredit = 0;

                            _cdb.TicketSaleRows.Add(saleRow);
                            _cdb.SaveChanges();

                        }

                        //payment eklenir

                        TicketSalePosPayment payment = new TicketSalePosPayment();

                        payment.SaleId = ticketSale.Id;
                        payment.FromPosTerminal = localPosPayment?.FromPosTerminal ?? false;
                        payment.PaymentType = localPosPayment?.PaymentType == 1 ? 1 : localPosPayment?.PaymentType == 2 ? 4 : 1;
                        payment.PaymentSubType = string.Empty;
                        payment.NumberOfInstallment = localPosPayment?.NumberOfInstallment ?? 0;
                        payment.PaymentAmount = localPosPayment?.PaymentAmount ?? 0;
                        payment.PaymentDesc = string.Empty;
                        payment.PaymentCurrency = localPosPayment?.Currency == "USD" ? 1 : 1;
                        payment.PaymentInfo = null;
                        payment.PaymentDateTime = localPosPayment?.PaymentDate.ToString();
                        payment.PaymentDate = (localPosPayment?.PaymentDate ?? DateTime.Now).Date;
                        payment.PaymentTime = (localPosPayment?.PaymentDate ?? DateTime.Now).TimeOfDay;
                        payment.BankBkmid = 0;
                        payment.BatchNumber = string.Empty;
                        payment.StanNumber = string.Empty;
                        payment.MerchantId = string.Empty;
                        payment.TerminalId = string.Empty;
                        payment.ReferenceNumber = localOrder.OrderNumber;
                        payment.AuthorizationCode = null;
                        payment.MaskedPan = string.Empty;
                        payment.RecordDate = localPosPayment?.RecordDate;

                        _cdb.TicketSalePosPayments.Add(payment);
                        _cdb.SaveChanges();

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                        // cari hareket işlenir CheckLocationPosTicketSale

                        var orderSvc = _sp.GetService<ISaleOrderService>();
                        orderSvc?.CheckLocationPosTicketSale(ticketSale.Id);


                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            if (process.Entity == "OrderRow" && process.Process == 2)
            {

                var localOrderRow = _db.OrderRows.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                var cloudOrderRow = _cdb.TicketSaleRows.FirstOrDefault(x => x.Uid == process.EntityUid && x.LocalRowId == process.EntityId);

                try
                {
                    if (cloudOrderRow != null && localOrderRow != null)
                    {
                        var localConfirm = _db.TripConfirms.FirstOrDefault(x => x.SaleOrderRowId == process.EntityId);

                        if (localConfirm != null)
                        {

                            TicketTripConfirm confirm = _cdb.TicketTripConfirms.FirstOrDefault(x => x.ConfirmNumber == localConfirm.ConfirmNumber && x.TicketNumber == localConfirm.TicketNumber);

                            if (confirm != null)
                            {
                                confirm.LocationId = localConfirm.LocationId;
                                confirm.EmployeeId = localConfirm.EmployeeId;
                                confirm.RecordDate = localConfirm.RecordDate;
                                confirm.ConfirmNumber = localConfirm.ConfirmNumber;
                                confirm.TicketSaleId = cloudOrderRow.SaleId;
                                confirm.TicketSaleRowId = cloudOrderRow.Id;
                                confirm.ConfirmTime = localConfirm.ConfirmTime;
                                confirm.TicketNumber = localConfirm.TicketNumber;
                                confirm.UnitDuration = localConfirm.UnitDuration;
                                confirm.RecordDate = localConfirm.RecordDate;
                                confirm.IsApproved = localConfirm.IsApproved;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                confirm = new TicketTripConfirm();

                                confirm.LocationId = localConfirm.LocationId;
                                confirm.EmployeeId = localConfirm.EmployeeId;
                                confirm.RecordDate = localConfirm.RecordDate;
                                confirm.ConfirmNumber = localConfirm.ConfirmNumber;
                                confirm.TicketSaleId = cloudOrderRow.SaleId;
                                confirm.TicketSaleRowId = cloudOrderRow.Id;
                                confirm.ConfirmTime = localConfirm.ConfirmTime;
                                confirm.TicketNumber = localConfirm.TicketNumber;
                                confirm.UnitDuration = localConfirm.UnitDuration;
                                confirm.RecordDate = localConfirm.RecordDate;
                                confirm.IsApproved = localConfirm.IsApproved;

                                _cdb.TicketTripConfirms.Add(confirm);
                                _cdb.SaveChanges();
                            }

                            var localTrip = _db.Trips.FirstOrDefault(x => x.ConfirmId == localConfirm.Id);




                            if (localTrip != null)
                            {
                                TicketTrip trip = _cdb.TicketTrips.FirstOrDefault(x => x.ConfirmId == confirm.Id);

                                if (trip != null)
                                {
                                    trip.RecordDate = localTrip.RecordDate;
                                    trip.TripDate = localTrip.TripDate;
                                    trip.TripStart = localTrip.TripStart;
                                    trip.TripCancel = localTrip.TripCancel;
                                    trip.TripEnd = localTrip.TripEnd;

                                    _cdb.SaveChanges();

                                }
                                else
                                {
                                    trip = new TicketTrip();

                                    trip.ConfirmId = confirm.Id;
                                    trip.LocationId = localTrip.LocationId;
                                    trip.EmployeeId = localTrip.EmployeeId;
                                    trip.TicketTypeId = cloudOrderRow.TicketTypeId;
                                    trip.TicketNumber = localTrip.TicketNumber;
                                    trip.RecordDate = localTrip.RecordDate;
                                    trip.TripDate = localTrip.TripDate;
                                    trip.TripStart = localTrip.TripStart;
                                    trip.TripCancel = localTrip.TripCancel;
                                    trip.TripEnd = localTrip.TripEnd;

                                    _cdb.TicketTrips.Add(trip);
                                    _cdb.SaveChanges();

                                }

                                cloudOrderRow.TicketTripId = trip.Id;
                            }



                        }

                        cloudOrderRow.StatusId = localOrderRow.RowStatusId;
                        _cdb.SaveChanges();


                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);

                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            if (process.Entity == "OrderRefund" && process.Process == 3)
            {

                var localOrder = _db.Orders.FirstOrDefault(x => x.Id == process.EntityId);

                var localOrderRows = _db.OrderRows.Where(x => x.OrderId == localOrder.Id).ToList();

                var localPosPayment = _db.OrderPosPayments.FirstOrDefault(x => x.OrderId == process.EntityId);
                var localPosRefund = _db.OrderPosRefunds.FirstOrDefault(x => x.OrderId == process.EntityId);

                var ticketSale = _cdb.TicketSales.FirstOrDefault(x => x.Uid == process.EntityUid && x.LocalOrderId == process.EntityId);

                try
                {
                    if (ticketSale != null)
                    {


                        ticketSale.StatusId = localOrder.OrderStatusId;
                        ticketSale.UpdateDate = localOrder.UpdateDate;
                        ticketSale.UpdateEmployeeId = localOrder.UpdateEmployeeId;
                        _cdb.SaveChanges();

                        //ticketsalerows güncellenir
                        var ticketSaleRows = _cdb.TicketSaleRows.Where(x => x.SaleId == ticketSale.Id).ToList();

                        foreach (var saleRow in ticketSaleRows)
                        {

                            var localrow = localOrderRows.FirstOrDefault(x => x.Id == saleRow.LocalRowId);

                            saleRow.StatusId = localrow.RowStatusId;
                            saleRow.UpdateDate = localrow.UpdateDate;
                            saleRow.UpdateEmployeeId = localrow.UpdateEmployeeId;

                            _cdb.SaveChanges();

                        }

                        //cloud a refund eklenir

                        var dayresult = _cdb.DayResults.FirstOrDefault(x => x.LocationId == localOrder.LocationId && x.Date == localOrder.Date && x.IsActive == true);

                        DocumentExpenseSlip slip = _cdb.DocumentExpenseSlips.FirstOrDefault(x => x.LocationId == localOrder.LocationId && x.DocumentDate == localPosRefund.RefoundDate && x.SaleId == ticketSale.Id);

                        if (slip == null)
                        {
                            slip = new DocumentExpenseSlip();

                            slip.ActionTypeId = 41;
                            slip.OurCompanyId = 1;
                            slip.LocationId = localOrder.LocationId;
                            slip.DocumentDate = localPosRefund?.RefoundDate;
                            slip.DocumentNumber = localOrder.OrderNumber;
                            slip.CustomerId = null;
                            slip.CustomerAddress = null;
                            slip.PayMethodId = localPosRefund.RefundType;
                            slip.Amount = localPosRefund.RefundAmount;
                            slip.Currency = localPosRefund.Currency;
                            slip.ExchangeRate = 1;
                            slip.SystemAmount = localPosRefund.RefundAmount;
                            slip.SystemCurrency = localPosRefund.Currency;
                            slip.ReferenceId = ticketSale.Id;
                            slip.SaleId = ticketSale.Id;
                            slip.SaleRowId = 0;
                            slip.ActionTypeName = "Refund Slip";
                            slip.Description = localPosRefund.Description;
                            slip.ResultId = dayresult.Id;
                            slip.EnvironmentId = 7;
                            slip.Uid = localPosRefund.Uid;
                            slip.RecordDate = localPosRefund.RecordDate;
                            slip.RecordEmployeeId = localPosRefund.RecordEmployeeId;
                            slip.IsConfirmed = true;
                            slip.IsActive = true;

                            _cdb.DocumentExpenseSlips.Add(slip);
                            _cdb.SaveChanges();
                        }

                        var parameterSlipID = new SqlParameter
                        {
                            ParameterName = "@SlipID",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Value = slip.Id

                        };

                        var sqlorder = "EXEC ExpenseSlipCheck @SlipID";
                        _cdb.Database.ExecuteSqlRaw(sqlorder, parameterSlipID);

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "CashDocument" && process.Process == 1)
            {
                var localDocument = _db.CashDocuments.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);
                var localAction = _db.CashActions.FirstOrDefault(x => x.ProcessUid == process.EntityUid && x.ProcessId == process.EntityId);
                var location = _db.OurLocations.FirstOrDefault();

                try
                {
                    if (localDocument != null && localAction != null)
                    {
                        if (localDocument.CashActionTypeId == 29)
                        {

                            var exist = _cdb.DocumentCashExpenses.FirstOrDefault(x => x.Uid == localDocument.Uid);

                            if (exist == null)
                            {
                                var parameterDocumentNumber = new SqlParameter
                                {
                                    ParameterName = "@DocumentNumber",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Direction = System.Data.ParameterDirection.Output,
                                    Size = 50,
                                };

                                var parameterOurCompanyId = new SqlParameter
                                {
                                    ParameterName = "@OurCompanyID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = 1,
                                };

                                var parameterPrefix = new SqlParameter
                                {
                                    ParameterName = "@Prefix",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Size = 4,
                                    Value = "EXP",
                                };

                                var sqldoc = "EXEC GetDocumentNumberForApp @OurCompanyID, @Prefix, @DocumentNumber OUT";
                                _cdb.Database.ExecuteSqlRaw(sqldoc, parameterOurCompanyId, parameterPrefix, parameterDocumentNumber);

                                string documentNumber = (string)parameterDocumentNumber.Value;

                                DocumentCashExpense cashExpense = new DocumentCashExpense();

                                cashExpense.ActionTypeId = localDocument.CashActionTypeId;
                                cashExpense.ActionTypeName = "Expense Document";
                                cashExpense.Amount = localDocument.Amount;
                                cashExpense.CashId = localAction.CashId;
                                cashExpense.Currency = localDocument.Currency;
                                cashExpense.Date = localDocument.DocumentDate;
                                cashExpense.Description = localDocument.Description;
                                cashExpense.DocumentNumber = documentNumber;
                                cashExpense.ExchangeRate = 1;
                                cashExpense.ToBankAccountId = (int?)null;
                                cashExpense.ToEmployeeId = (int?)null;
                                cashExpense.ToCustomerId = (int?)null;
                                cashExpense.IsActive = true;
                                cashExpense.LocationId = location.Id;
                                cashExpense.OurCompanyId = location.OurCompanyId;
                                cashExpense.RecordDate = location.LocalDateTime;
                                cashExpense.RecordEmployeeId = localDocument.RecordEmployeeId;
                                cashExpense.RecordIp = string.Empty;
                                cashExpense.SystemAmount = localDocument.Amount;
                                cashExpense.SystemCurrency = location.Currency;
                                cashExpense.SlipNumber = string.Empty;
                                cashExpense.SlipDate = localDocument.DocumentDate;
                                cashExpense.ReferenceId = localDocument.Id;
                                cashExpense.EnvironmentId = 4;
                                cashExpense.Uid = localDocument.Uid;
                                cashExpense.ExpenseTypeId = 7;
                                cashExpense.SlipPath = "/Document/Expense";
                                cashExpense.SlipDocument = localDocument.PhotoFile;

                                _cdb.DocumentCashExpenses.Add(cashExpense);
                                _cdb.SaveChanges();

                                // Cash Action Eklenir

                                AddCashAction(cashExpense.CashId, cashExpense.LocationId, cashExpense.RecordEmployeeId, cashExpense.ActionTypeId, cashExpense.Date, cashExpense.ActionTypeName, cashExpense.Id, cashExpense.Date, cashExpense.DocumentNumber, cashExpense.Description, -1, 0, cashExpense.Amount, cashExpense.Currency, cashExpense.RecordEmployeeId, cashExpense.RecordDate, cashExpense.Uid.Value);

                            }


                        }

                        if (localDocument.CashActionTypeId == 31)
                        {

                            var exist = _cdb.DocumentSalaryPayments.FirstOrDefault(x => x.Uid == localDocument.Uid);

                            if (exist == null)
                            {
                                var parameterDocumentNumber = new SqlParameter
                                {
                                    ParameterName = "@DocumentNumber",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Direction = System.Data.ParameterDirection.Output,
                                    Size = 50,
                                };

                                var parameterOurCompanyId = new SqlParameter
                                {
                                    ParameterName = "@OurCompanyID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = 1,
                                };

                                var parameterPrefix = new SqlParameter
                                {
                                    ParameterName = "@Prefix",
                                    SqlDbType = System.Data.SqlDbType.NVarChar,
                                    Size = 4,
                                    Value = "SAP",
                                };

                                var sqldoc = "EXEC GetDocumentNumberForApp @OurCompanyID, @Prefix, @DocumentNumber OUT";
                                _cdb.Database.ExecuteSqlRaw(sqldoc, parameterOurCompanyId, parameterPrefix, parameterDocumentNumber);

                                string documentNumber = (string)parameterDocumentNumber.Value;

                                //DayResultId

                                var parameterDayResultID = new SqlParameter
                                {
                                    ParameterName = "@DayResultID",
                                    SqlDbType = System.Data.SqlDbType.BigInt,
                                    Direction = System.Data.ParameterDirection.Output
                                };

                                var parameterLocationID = new SqlParameter
                                {
                                    ParameterName = "@LocationID",
                                    SqlDbType = System.Data.SqlDbType.Int,
                                    Value = location.Id,
                                };

                                var parameterDate = new SqlParameter
                                {
                                    ParameterName = "@Date",
                                    SqlDbType = System.Data.SqlDbType.Date,
                                    Value = location.LocalDate
                                };

                                var sqlres = "EXEC GetDayResultIDApp @LocationID, @Date, @DayResultID OUT";
                                _cdb.Database.ExecuteSqlRaw(sqlres, parameterLocationID, parameterDate, parameterDayResultID);

                                long dayResultId = (long)parameterDayResultID.Value;


                                DocumentSalaryPayment salaryPay = new DocumentSalaryPayment();

                                salaryPay.ActionTypeId = 31;
                                salaryPay.ActionTypeName = "Advance Payment Document";
                                salaryPay.Amount = localDocument.Amount;
                                salaryPay.FromCashId = localAction.CashId;
                                salaryPay.Currency = localDocument.Currency;
                                salaryPay.Date = localDocument.DocumentDate;
                                salaryPay.Description = localDocument.Description;
                                salaryPay.DocumentNumber = documentNumber;
                                salaryPay.ExchangeRate = 1;
                                salaryPay.ToEmployeeId = localDocument.RecordEmployeeId;
                                salaryPay.IsActive = true;
                                salaryPay.LocationId = location.Id;
                                salaryPay.OurCompanyId = location.OurCompanyId;
                                salaryPay.RecordDate = location.LocalDateTime;
                                salaryPay.RecordEmployeeId = localDocument.RecordEmployeeId;
                                salaryPay.RecordIp = string.Empty;
                                salaryPay.SystemAmount = localDocument.Amount;
                                salaryPay.SystemCurrency = location.Currency;
                                salaryPay.SalaryTypeId = 2;
                                salaryPay.ReferenceId = localDocument.Id;
                                salaryPay.EnvironmentId = 4;
                                salaryPay.Uid = localDocument.Uid;
                                salaryPay.ResultId = dayResultId;


                                _cdb.DocumentSalaryPayments.Add(salaryPay);
                                _cdb.SaveChanges();

                                // Cash Action Eklenir

                                AddCashAction(salaryPay.FromCashId, salaryPay.LocationId, salaryPay.RecordEmployeeId, salaryPay.ActionTypeId, salaryPay.Date, salaryPay.ActionTypeName, salaryPay.Id, salaryPay.Date, salaryPay.DocumentNumber, salaryPay.Description, -1, 0, salaryPay.Amount, salaryPay.Currency, salaryPay.RecordEmployeeId, salaryPay.RecordDate, salaryPay.Uid.Value);
                                AddEmployeeAction(
                                    salaryPay.RecordEmployeeId,
                                    salaryPay.LocationId,
                                    salaryPay.ActionTypeId,
                                    salaryPay.ActionTypeName,
                                    salaryPay.Id,
                                    salaryPay.Date,
                                    salaryPay.Description,
                                    1,
                                    0,
                                    salaryPay.Amount,
                                    salaryPay.Currency,
                                    2,
                                    salaryPay.RecordEmployeeId,
                                    salaryPay.RecordDate,
                                    salaryPay.Uid.Value,
                                    salaryPay.DocumentNumber,
                                    8
                                    );
                            }


                        }

                        // image upload edilir.

                        if (localDocument.PhotoFile != null && !string.IsNullOrEmpty(localDocument.PhotoFile) && !string.IsNullOrEmpty(process.FilePath))
                        {
                            try
                            {
                                string imagePath = process.FilePath;
                                byte[] imageBytes = File.ReadAllBytes(imagePath);
                                ServiceHelper.UploadToFtp(imageBytes, localDocument.PhotoFile, "Document/Expense");

                            }
                            catch (Exception ex)
                            {
                            }

                        }


                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "DayResult" && process.Process == 1)
            {

                //var location = db.OurLocations.FirstOrDefault();
                var dayResult = _db.DayResults.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (dayResult != null)
                    {
                        var exist = _cdb.DayResults.FirstOrDefault(x => x.Uid == dayResult.Uid);

                        if (exist == null)
                        {
                            var parameterDayResultID = new SqlParameter
                            {
                                ParameterName = "@DayResultID",
                                SqlDbType = System.Data.SqlDbType.BigInt,
                                Direction = System.Data.ParameterDirection.Output,
                            };

                            var parameterLocationID = new SqlParameter
                            {
                                ParameterName = "@LocationID",
                                SqlDbType = System.Data.SqlDbType.Int,
                                Value = dayResult.LocationId,
                            };

                            var parameterDate = new SqlParameter
                            {
                                ParameterName = "@Date",
                                SqlDbType = System.Data.SqlDbType.Date,
                                Value = dayResult.Date,
                            };

                            var parameterStateID = new SqlParameter
                            {
                                ParameterName = "@StateID",
                                SqlDbType = System.Data.SqlDbType.Int,
                                Value = dayResult.StateId,
                            };

                            var parameterEnvironmentID = new SqlParameter
                            {
                                ParameterName = "@EnvironmentID",
                                SqlDbType = System.Data.SqlDbType.Int,
                                Value = dayResult.EnvironmentId,
                            };

                            var parameterRecordEmployeeID = new SqlParameter
                            {
                                ParameterName = "@RecordEmployeeID",
                                SqlDbType = System.Data.SqlDbType.Int,
                                Value = dayResult.RecordEmployeeId,
                            };

                            var parameterDescription = new SqlParameter
                            {
                                ParameterName = "@Description",
                                SqlDbType = System.Data.SqlDbType.NVarChar,
                                Size = 1000,
                                Value = dayResult.Description,
                            };

                            var parameterRecordIP = new SqlParameter
                            {
                                ParameterName = "@RecordIP",
                                SqlDbType = System.Data.SqlDbType.NVarChar,
                                Size = 20,
                                Value = "localhost",
                            };

                            var parameterUID = new SqlParameter
                            {
                                ParameterName = "@UID",
                                SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                                Value = dayResult.Uid,
                            };


                            var sqldoc = "EXEC AddDayResultApp @LocationID, @Date, @StateID, @EnvironmentID, @RecordEmployeeID, @Description, @RecordIP, @UID, @DayResultID OUT";
                            _cdb.Database.ExecuteSqlRaw(sqldoc, parameterLocationID, parameterDate, parameterStateID, parameterEnvironmentID, parameterRecordEmployeeID, parameterDescription, parameterRecordIP, parameterUID, parameterDayResultID);


                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "DayResult" && process.Process == 2)
            {

                // var location = db.OurLocations.FirstOrDefault();
                var dayResult = _db.DayResults.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (dayResult != null)
                    {
                        var exist = _cdb.DayResults.FirstOrDefault(x => x.Uid == dayResult.Uid);

                        if (exist != null)
                        {
                            exist.StateId = dayResult.StateId;
                            exist.Description = dayResult.Description;
                            exist.UpdateDate = dayResult.UpdateDate;
                            exist.UpdateEmployeeId = dayResult.UpdateEmployeeId;

                            _cdb.SaveChanges();


                            // image upload edilir.

                            if (dayResult.PhotoFile != null && !string.IsNullOrEmpty(dayResult.PhotoFile) && !string.IsNullOrEmpty(process.FilePath))
                            {
                                try
                                {
                                    string imagePath = process.FilePath;
                                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                                    ServiceHelper.UploadToFtp(imageBytes, dayResult.PhotoFile, "Document/Envelope");


                                    DayResultDocument doc = new DayResultDocument();

                                    doc.ResultId = exist.Id;
                                    doc.DocumentTypeId = 1;
                                    doc.LocationId = exist.LocationId;
                                    doc.Date = exist.Date;
                                    doc.EnvironmentId = exist.EnvironmentId;
                                    doc.FilePath = "/Document/Envelope";
                                    doc.FileName = dayResult.PhotoFile;
                                    doc.Description = string.Empty;
                                    doc.RecordDate = dayResult.UpdateDate;
                                    doc.RecordEmployeeId = dayResult.UpdateEmployeeId;
                                    doc.IsActive = true;
                                    doc.RecordIp = string.Empty;

                                    _cdb.DayResultDocuments.Add(doc);
                                    _cdb.SaveChanges();

                                }
                                catch (Exception ex)
                                {
                                }

                            }

                            // sync silinir

                            var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                            if (syncOrder != null)
                            {
                                _db.SyncProcesses.Remove(syncOrder);
                                _db.SaveChanges();
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "LocationShift" && process.Process == 1)
            {
                var locationShift = _db.LocationShifts.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (locationShift != null)
                    {
                        var exist = _cdb.LocationShifts.FirstOrDefault(x => x.Uid == locationShift.Uid && x.LocationId == locationShift.LocationId && x.ShiftDate == locationShift.ShiftDate);

                        if (exist != null)
                        {
                            exist.ShiftDateStart = locationShift.ShiftStart;
                            exist.ShiftDateFinish = locationShift.ShiftFinish;
                            exist.ShiftStart = locationShift.ShiftStart.TimeOfDay;
                            exist.ShiftFinish = locationShift.ShiftFinish?.TimeOfDay;


                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = _cdb.LocationShifts.FirstOrDefault(x => x.LocationId == locationShift.LocationId && x.ShiftDate == locationShift.ShiftDate);

                            if (exist != null)
                            {
                                exist.ShiftDateStart = locationShift.ShiftStart;
                                exist.ShiftDateFinish = locationShift.ShiftFinish;
                                exist.ShiftStart = locationShift.ShiftStart.TimeOfDay;
                                exist.ShiftFinish = locationShift.ShiftFinish?.TimeOfDay;
                                exist.Uid= locationShift.Uid;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                exist = new DataCloud.Entities.LocationShift();

                                exist.ShiftDateStart = locationShift.ShiftStart;
                                exist.ShiftDateFinish = locationShift.ShiftFinish;
                                exist.ShiftStart = locationShift.ShiftStart.TimeOfDay;
                                exist.ShiftFinish = locationShift.ShiftFinish?.TimeOfDay;
                                exist.LocationId = locationShift.LocationId;
                                exist.EmployeeId = locationShift.EmployeeId;
                                exist.ShiftDate = locationShift.ShiftDate;
                                exist.LatitudeFinish = 0;
                                exist.LongitudeFinish = 0;
                                exist.FromMobileStart = true;
                                exist.RecordEmployeeId = locationShift.RecordEmployeeId;
                                exist.RecordDate = locationShift.RecordDate;
                                exist.EnvironmentId = 4;
                                exist.Uid = locationShift.Uid;

                                _cdb.LocationShifts.Add(exist);
                                _cdb.SaveChanges();
                            }
                            
                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            if (process.Entity == "LocationShift" && process.Process == 2)
            {
                var location = _db.OurLocations.FirstOrDefault();
                var locationShift = _db.LocationShifts.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (locationShift != null)
                    {
                        var exist = _cdb.LocationShifts.FirstOrDefault(x => x.Uid == locationShift.Uid && x.LocationId == locationShift.LocationId && x.ShiftDate == locationShift.ShiftDate);

                        if (exist != null)
                        {
                            exist.ShiftDateStart = locationShift.ShiftStart;
                            exist.ShiftDateFinish = locationShift.ShiftFinish;
                            exist.ShiftStart = locationShift.ShiftStart.TimeOfDay;
                            exist.ShiftFinish = locationShift.ShiftFinish?.TimeOfDay;

                            exist.UpdateDate = location.LocalDateTime;
                            exist.UpdateEmployeeId = locationShift.EmployeeId;


                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = _cdb.LocationShifts.FirstOrDefault(x => x.LocationId == locationShift.LocationId && x.ShiftDate == locationShift.ShiftDate);

                            if (exist != null)
                            {
                                exist.ShiftDateStart = locationShift.ShiftStart;
                                exist.ShiftDateFinish = locationShift.ShiftFinish;
                                exist.ShiftStart = locationShift.ShiftStart.TimeOfDay;
                                exist.ShiftFinish = locationShift.ShiftFinish?.TimeOfDay;
                                exist.Uid = locationShift.Uid;
                                exist.UpdateDate = location.LocalDateTime;
                                exist.UpdateEmployeeId = locationShift.EmployeeId;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                exist = new DataCloud.Entities.LocationShift();

                                exist.ShiftDateStart = locationShift.ShiftStart;
                                exist.ShiftDateFinish = locationShift.ShiftFinish;
                                exist.ShiftStart = locationShift.ShiftStart.TimeOfDay;
                                exist.ShiftFinish = locationShift.ShiftFinish?.TimeOfDay;
                                exist.LocationId = locationShift.LocationId;
                                exist.EmployeeId = locationShift.EmployeeId;
                                exist.ShiftDate = locationShift.ShiftDate;
                                exist.LatitudeFinish = 0;
                                exist.LongitudeFinish = 0;
                                exist.FromMobileStart = true;
                                exist.RecordEmployeeId = locationShift.RecordEmployeeId;
                                exist.RecordDate = locationShift.RecordDate;
                                exist.UpdateDate = location.LocalDateTime;
                                exist.UpdateEmployeeId = locationShift.EmployeeId;
                                exist.EnvironmentId = 4;
                                exist.Uid = locationShift.Uid;

                                _cdb.LocationShifts.Add(exist);
                                _cdb.SaveChanges();
                            }

                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "EmployeeShift" && process.Process == 1)
            {

                var employeeShift = _db.EmployeeShifts.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (employeeShift != null)
                    {
                        var exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.Uid == employeeShift.Uid && x.EmployeeId == employeeShift.EmployeeId && x.ShiftDate == employeeShift.ShiftDate && x.IsWorkTime == true);

                        if (exist != null)
                        {
                            exist.ShiftDateStart = employeeShift.ShiftStart;
                            exist.ShiftDateEnd = employeeShift.ShiftEnd;
                            exist.ShiftStart = employeeShift.ShiftStart.TimeOfDay;
                            exist.ShiftEnd = employeeShift.ShiftEnd?.TimeOfDay;

                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeShift.EmployeeId && x.ShiftDate == employeeShift.ShiftDate && x.IsWorkTime == true);

                            if (exist != null)
                            {
                                exist.ShiftDateStart = employeeShift.ShiftStart;
                                exist.ShiftDateEnd = employeeShift.ShiftEnd;
                                exist.ShiftStart = employeeShift.ShiftStart.TimeOfDay;
                                exist.ShiftEnd = employeeShift.ShiftEnd?.TimeOfDay;
                                exist.Uid = employeeShift.Uid;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                exist = new DataCloud.Entities.EmployeeShift();

                                exist.ShiftDateStart = employeeShift.ShiftStart;
                                exist.ShiftDateEnd = employeeShift.ShiftEnd;
                                exist.ShiftStart = employeeShift.ShiftStart.TimeOfDay;
                                exist.ShiftEnd = employeeShift.ShiftEnd?.TimeOfDay;
                                exist.LocationId = employeeShift.LocationId;
                                exist.EmployeeId = employeeShift.EmployeeId;
                                exist.ShiftDate = employeeShift.ShiftDate;
                                exist.IsWorkTime = true;
                                exist.IsBreakTime = null;
                                exist.LatitudeFinish = 0;
                                exist.LongitudeFinish = 0;
                                exist.FromMobileStart = true;
                                exist.RecordEmployeeId = employeeShift.RecordEmployeeId;
                                exist.RecordDate = employeeShift.RecordDate;
                                exist.EnvironmentId = 4;
                                exist.Uid = employeeShift.Uid;

                                _cdb.EmployeeShifts.Add(exist);
                                _cdb.SaveChanges();
                            }

                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "EmployeeShift" && process.Process == 2)
            {

                var location = _db.OurLocations.FirstOrDefault();
                var employeeShift = _db.EmployeeShifts.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (employeeShift != null)
                    {
                        var exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.Uid == employeeShift.Uid && x.EmployeeId == employeeShift.EmployeeId && x.ShiftDate == employeeShift.ShiftDate && x.IsWorkTime == true);

                        if (exist != null)
                        {
                            exist.ShiftDateStart = employeeShift.ShiftStart;
                            exist.ShiftDateEnd = employeeShift.ShiftEnd;
                            exist.ShiftStart = employeeShift.ShiftStart.TimeOfDay;
                            exist.ShiftEnd = employeeShift.ShiftEnd?.TimeOfDay;

                            exist.UpdateDate = location.LocalDateTime;
                            exist.UpdateEmployeeId = employeeShift.EmployeeId;

                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeShift.EmployeeId && x.ShiftDate == employeeShift.ShiftDate && x.IsWorkTime == true);

                            if (exist != null)
                            {
                                exist.ShiftDateStart = employeeShift.ShiftStart;
                                exist.ShiftDateEnd = employeeShift.ShiftEnd;
                                exist.ShiftStart = employeeShift.ShiftStart.TimeOfDay;
                                exist.ShiftEnd = employeeShift.ShiftEnd?.TimeOfDay;
                                exist.Uid = employeeShift.Uid;
                                exist.UpdateDate = location.LocalDateTime;
                                exist.UpdateEmployeeId = employeeShift.EmployeeId;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                exist = new DataCloud.Entities.EmployeeShift();

                                exist.ShiftDateStart = employeeShift.ShiftStart;
                                exist.ShiftDateEnd = employeeShift.ShiftEnd;
                                exist.ShiftStart = employeeShift.ShiftStart.TimeOfDay;
                                exist.ShiftEnd = employeeShift.ShiftEnd?.TimeOfDay;
                                exist.LocationId = employeeShift.LocationId;
                                exist.EmployeeId = employeeShift.EmployeeId;
                                exist.ShiftDate = employeeShift.ShiftDate;
                                exist.IsWorkTime = true;
                                exist.IsBreakTime = null;
                                exist.LatitudeFinish = 0;
                                exist.LongitudeFinish = 0;
                                exist.FromMobileStart = true;
                                exist.RecordEmployeeId = employeeShift.RecordEmployeeId;
                                exist.RecordDate = employeeShift.RecordDate;
                                exist.UpdateDate = location.LocalDateTime;
                                exist.UpdateEmployeeId = employeeShift.EmployeeId;
                                exist.EnvironmentId = 4;
                                exist.Uid = employeeShift.Uid;

                                _cdb.EmployeeShifts.Add(exist);
                                _cdb.SaveChanges();
                            }

                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            if (process.Entity == "EmployeeBreak" && process.Process == 1)
            {


                var employeeBreak = _db.EmployeeBreaks.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (employeeBreak != null)
                    {
                        var exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.Uid == employeeBreak.Uid && x.EmployeeId == employeeBreak.EmployeeId && x.ShiftDate == employeeBreak.BreakDate && x.IsBreakTime == true);

                        if (exist != null)
                        {
                            exist.BreakDateStart = employeeBreak.BreakStart;
                            exist.BreakDateEnd = employeeBreak.BreakEnd;
                            exist.BreakStart = employeeBreak.BreakStart.TimeOfDay;
                            exist.BreakEnd = employeeBreak.BreakEnd?.TimeOfDay;

                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeBreak.EmployeeId && x.ShiftDate == employeeBreak.BreakDate && x.IsBreakTime == true);

                            if (exist != null)
                            {
                                exist.BreakDateStart = employeeBreak.BreakStart;
                                exist.BreakDateEnd = employeeBreak.BreakEnd;
                                exist.BreakStart = employeeBreak.BreakStart.TimeOfDay;
                                exist.BreakEnd = employeeBreak.BreakEnd?.TimeOfDay;
                                exist.Uid = employeeBreak.Uid;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                exist = new DataCloud.Entities.EmployeeShift();

                                exist.BreakDateStart = employeeBreak.BreakStart;
                                exist.BreakDateEnd = employeeBreak.BreakEnd;
                                exist.BreakStart = employeeBreak.BreakStart.TimeOfDay;
                                exist.BreakEnd = employeeBreak.BreakEnd?.TimeOfDay;
                                exist.LocationId = employeeBreak.LocationId;
                                exist.EmployeeId = employeeBreak.EmployeeId;
                                exist.ShiftDate = employeeBreak.BreakDate;
                                exist.IsWorkTime = null;
                                exist.IsBreakTime = true;
                                exist.LatitudeFinish = 0;
                                exist.LongitudeFinish = 0;
                                exist.FromMobileStart = true;
                                exist.RecordEmployeeId = employeeBreak.RecordEmployeeId;
                                exist.RecordDate = employeeBreak.RecordDate;
                                exist.EnvironmentId = 4;
                                exist.Uid = employeeBreak.Uid;

                                _cdb.EmployeeShifts.Add(exist);
                                _cdb.SaveChanges();
                            }

                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            }

            if (process.Entity == "EmployeeBreak" && process.Process == 2)
            {

                var location = _db.OurLocations.FirstOrDefault();
                var employeeBreak = _db.EmployeeBreaks.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (employeeBreak != null)
                    {
                        var exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.Uid == employeeBreak.Uid && x.EmployeeId == employeeBreak.EmployeeId && x.ShiftDate == employeeBreak.BreakDate && x.IsBreakTime == true);

                        if (exist != null)
                        {
                            exist.BreakDateStart = employeeBreak.BreakStart;
                            exist.BreakDateEnd = employeeBreak.BreakEnd;
                            exist.BreakStart = employeeBreak.BreakStart.TimeOfDay;
                            exist.BreakEnd = employeeBreak.BreakEnd?.TimeOfDay;

                            exist.UpdateDate = location.LocalDateTime;
                            exist.UpdateEmployeeId = employeeBreak.EmployeeId;

                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = _cdb.EmployeeShifts.FirstOrDefault(x => x.EmployeeId == employeeBreak.EmployeeId && x.ShiftDate == employeeBreak.BreakDate && x.IsBreakTime == true);

                            if (exist != null)
                            {
                                exist.BreakDateStart = employeeBreak.BreakStart;
                                exist.BreakDateEnd = employeeBreak.BreakEnd;
                                exist.BreakStart = employeeBreak.BreakStart.TimeOfDay;
                                exist.BreakEnd = employeeBreak.BreakEnd?.TimeOfDay;
                                exist.Uid = employeeBreak.Uid;
                                exist.UpdateDate = location.LocalDateTime;
                                exist.UpdateEmployeeId = employeeBreak.EmployeeId;

                                _cdb.SaveChanges();
                            }
                            else
                            {
                                exist = new DataCloud.Entities.EmployeeShift();

                                exist.BreakDateStart = employeeBreak.BreakStart;
                                exist.BreakDateEnd = employeeBreak.BreakEnd;
                                exist.BreakStart = employeeBreak.BreakStart.TimeOfDay;
                                exist.BreakEnd = employeeBreak.BreakEnd?.TimeOfDay;
                                exist.LocationId = employeeBreak.LocationId;
                                exist.EmployeeId = employeeBreak.EmployeeId;
                                exist.ShiftDate = employeeBreak.BreakDate;
                                exist.IsWorkTime = null;
                                exist.IsBreakTime = true;
                                exist.LatitudeFinish = 0;
                                exist.LongitudeFinish = 0;
                                exist.FromMobileStart = true;
                                exist.RecordEmployeeId = employeeBreak.RecordEmployeeId;
                                exist.RecordDate = employeeBreak.RecordDate;
                                exist.UpdateDate = location.LocalDateTime;
                                exist.UpdateEmployeeId = employeeBreak.EmployeeId;
                                exist.EnvironmentId = 4;
                                exist.Uid = employeeBreak.Uid;

                                _cdb.EmployeeShifts.Add(exist);
                                _cdb.SaveChanges();
                            }

                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }

            if (process.Entity == "Inspection" && process.Process == 1)
            {
                var inspection = _db.Inspections.FirstOrDefault(x => x.Id == process.EntityId && x.Uid == process.EntityUid);

                try
                {
                    if (inspection != null)
                    {
                        var exist = _cdb.Inspections.FirstOrDefault(x => x.Uid == inspection.Uid && x.LocationId == inspection.LocationId && x.InspectionDate == inspection.InspectionDate);

                        if (exist != null)
                        {
                            exist.InspectionDate = inspection.InspectionDate;
                            exist.LocationId = inspection.LocationId;
                            exist.RecordDate = inspection.RecordDate;
                            exist.Description = inspection.Description;
                            exist.DateEnd = inspection.DateEnd;
                            exist.DateBegin = inspection.DateBegin;
                            exist.InspectionTypeId = inspection.InspectionTypeId;
                            exist.InspectorId = inspection.InspectorId;
                            exist.LanguageCode = inspection.LanguageCode;
                            exist.Uid = inspection.Uid;
                            exist.RecordEmployeeId = inspection.RecordEmployeeId;

                            _cdb.SaveChanges();
                        }
                        else
                        {

                            exist = new DataCloud.Entities.Inspection();

                            exist.InspectionDate = inspection.InspectionDate;
                            exist.LocationId = inspection.LocationId;
                            exist.RecordDate = inspection.RecordDate;
                            exist.Description = inspection.Description;
                            exist.DateEnd = inspection.DateEnd;
                            exist.DateBegin = inspection.DateBegin;
                            exist.InspectionTypeId = inspection.InspectionTypeId;
                            exist.InspectorId = inspection.InspectorId;
                            exist.LanguageCode = inspection.LanguageCode;
                            exist.Uid = inspection.Uid;
                            exist.RecordEmployeeId = inspection.RecordEmployeeId;

                            _cdb.Inspections.Add(exist);
                            _cdb.SaveChanges();

                        }

                        // inspection rows eklenir
                        var inspectionRows = _db.VinspectionRows.Where(x => x.InspectionId == inspection.Id).ToList();

                        var cloudInspectionRows = _cdb.InspectionRows.Where(x => x.InspectionId == exist.Id).ToList();

                        if (cloudInspectionRows.Count > 0)
                        {
                            _cdb.InspectionRows.RemoveRange(cloudInspectionRows);
                            _cdb.SaveChanges(true);
                        }

                        List<DataCloud.Entities.InspectionRow> rows = new();

                        foreach (var row in inspectionRows)
                        {
                            var newrow = new DataCloud.Entities.InspectionRow();

                            newrow.InspectionId = exist.Id;
                            newrow.InspectionItemId = row.InspectionItemId;
                            newrow.InspectionCategoryId = row.InspectionCategoryId;
                            newrow.LocationPartId = row.PartialId ?? 0;
                            newrow.LanguageCode = row.LanguageCode;
                            newrow.InspectionItemName = row.InspectionItemName;
                            newrow.InspectionValue = row.InspectionValue;
                            newrow.EstimatedValue = row.EstimatedValue;
                            newrow.Description = row.Description;
                            newrow.InspectorId = row.InspectorId;
                            newrow.InpectionDate = row.InpectionDate;

                            rows.Add(newrow);
                        }


                        if (rows.Count > 0)
                        {
                            _cdb.InspectionRows.AddRange(rows);
                            _cdb.SaveChanges(true);
                        }

                        // sync silinir

                        var syncOrder = _db.SyncProcesses.FirstOrDefault(x => x.Id == process.Id);
                        if (syncOrder != null)
                        {
                            _db.SyncProcesses.Remove(syncOrder);
                            _db.SaveChanges();
                        }

                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            




        }


        private void AddCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        {

            var param1 = new SqlParameter("@CashID", CashID);
            var param2 = new SqlParameter("@LocationID", LocationID);
            var param3 = new SqlParameter("@EmployeeID", EmployeeID);
            var param4 = new SqlParameter("@CashActionTypeID", CashActionTypeID);
            var param5 = new SqlParameter("@ActionDate", ActionDate);
            var param6 = new SqlParameter("@ProcessName", ProcessName);
            var param7 = new SqlParameter("@ProcessID", ProcessID);
            var param8 = new SqlParameter("@ProcessDate", ProcessDate);
            var param9 = new SqlParameter("@DocumentNumber", DocumentNumber);
            var param10 = new SqlParameter("@Description", Description);
            var param11 = new SqlParameter("@Direction", Direction);
            var param12 = new SqlParameter("@Collection", Collection);
            var param13 = new SqlParameter("@Payment", Payment);
            var param14 = new SqlParameter("@Currency", Currency);
            var param15 = new SqlParameter("@Latitude", (double?)0);
            var param16 = new SqlParameter("@Longitude", (double?)0);
            var param17 = new SqlParameter("@RecordEmployeeID", RecordEmployeeID);
            var param18 = new SqlParameter("@RecordDate", RecordDate);
            var param19 = new SqlParameter("@ProcessUID", ProcessUID);

            var sqldoc = "EXEC AddCashAction @CashID, @LocationID, @EmployeeID, @CashActionTypeID, @ActionDate, @ProcessName, @ProcessID, @ProcessDate, @DocumentNumber, @Description, @Direction, @Collection, @Payment, @Currency, @Latitude, @Longitude, @RecordEmployeeID, @RecordDate, @ProcessUID ";
            _cdb.Database.ExecuteSqlRaw(sqldoc, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19);

        }


        private void AddEmployeeAction(int? EmployeeID, int? LocationID, int? ActionTypeID, string ProcessName, long? ProcessID, DateTime? ProcessDate, string ProcessDetail, short? Direction, double? Collection, double? Payment, string Currency, int? SalaryTypeID, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID, string DocumentNumber, int SalaryCategoryID)
        {

            var param1 = new SqlParameter("@EmployeeID", EmployeeID);
            var param2 = new SqlParameter("@LocationID", LocationID);
            var param3 = new SqlParameter("@ActionTypeID", ActionTypeID);
            var param4 = new SqlParameter("@ProcessName", ProcessName);
            var param5 = new SqlParameter("@ProcessID", ProcessID);
            var param6 = new SqlParameter("@ProcessDate", ProcessDate);
            var param7 = new SqlParameter("@ProcessDetail", ProcessDetail);
            var param8 = new SqlParameter("@Direction", Direction);
            var param9 = new SqlParameter("@Collection", Collection);
            var param10 = new SqlParameter("@Payment", Payment);
            var param11 = new SqlParameter("@Currency", Currency);
            var param12 = new SqlParameter("@Latitude", (double?)0);
            var param13 = new SqlParameter("@Longitude", (double?)0);
            var param14 = new SqlParameter("@SalaryTypeID", SalaryTypeID);
            var param15 = new SqlParameter("@RecordEmployeeID", RecordEmployeeID);
            var param16 = new SqlParameter("@RecordDate", RecordDate);
            var param17 = new SqlParameter("@ProcessUID", ProcessUID);
            var param18 = new SqlParameter("@DocumentNumber", DocumentNumber);
            var param19 = new SqlParameter("@SalaryCategoryID", SalaryCategoryID);

            var sqldoc = "EXEC AddEmployeeAction @EmployeeID, @LocationID, @ActionTypeID, @ProcessName, @ProcessID, @ProcessDate, @ProcessDetail, @Direction, @Collection, @Payment, @Currency, @Latitude, @Longitude, @SalaryTypeID, @RecordEmployeeID, @RecordDate, @ProcessUID, @DocumentNumber, @SalaryCategoryID ";
            _cdb.Database.ExecuteSqlRaw(sqldoc, param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12, param13, param14, param15, param16, param17, param18, param19);

        }



    }
}
