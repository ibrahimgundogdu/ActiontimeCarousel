using Actiontime.Data.Context;
using Actiontime.Data.Entities;
using Actiontime.DataCloud.Context;
using Actiontime.Models;
using Actiontime.Models.ResultModel;
using Actiontime.Models.SerializeModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class CashService
    {
        private readonly ApplicationDbContext _db;
        private readonly ApplicationCloudDbContext _cdb;
        CloudService _cloudService;

        public CashService()
        {
            _db = new ApplicationDbContext();
            _cdb = new ApplicationCloudDbContext();
            _cloudService = new CloudService();
        }

        public List<CashDocumentType> CashDocumentList()
        {
            var vardocuments = _db.CashActionTypes.Where(x => x.IsMobile == true).ToList().Select(x => new CashDocumentType() { Id = x.Id, DocumentName = x.MobileTag }).ToList();

            return vardocuments;
        }

        public AppResult AddDocument(string? FileName, int EmployeeId, DateTime DocDate, double Amount, string Description, int DocType, string FilePath)
        {
            var result = new AppResult() { Success = false, Message = string.Empty };

            var actionType = _db.CashActionTypes.FirstOrDefault(x => x.Id == DocType);

            if (actionType != null)
            {
                var location = _db.OurLocations.FirstOrDefault();
                var cash = _db.Cashes.FirstOrDefault(x => x.CashTypeId == 1 && x.LocationId == location.Id && x.IsMaster == true);

                CashDocument document = new CashDocument
                {
                    LocationId = location?.Id,
                    CashActionTypeId = actionType?.Id ?? 29,
                    PayMethodId = 1, // cash
                    Amount = Amount,
                    Currency = location?.Currency,
                    DocumentDate = DocDate,
                    Description = Description,
                    PhotoFile = FileName,
                    RecordDate = location.LocalDateTime,
                    RecordEmployeeId = EmployeeId,
                    Uid = Guid.NewGuid()
                };

                _db.CashDocuments.Add(document);
                _db.SaveChanges();

                CashAction cashAction = new CashAction
                {
                    CashId = cash?.Id,
                    CashActionTypeId = actionType?.Id,
                    LocationId = location?.Id,
                    ActionDate = DocDate,
                    ProcessId = document.Id,
                    Collection = 0,
                    Payment = Amount,
                    Currency = location?.Currency,
                    RecordEmployeeId = EmployeeId,
                    RecordDate = location?.LocalDateTime,
                    ProcessUid = document?.Uid
                };

                _db.CashActions.Add(cashAction);
                _db.SaveChanges();

                SyncProcess process = new SyncProcess()
                {
                    DateCreate = DateTime.Now,
                    Entity = "CashDocument",
                    Process = 1,
                    EntityId = document.Id,
                    EntityUid = document.Uid,
                    FilePath = FilePath,
                };

                _db.SyncProcesses.Add(process);
                _db.SaveChanges(true);

                CloudService _cloudService = new CloudService();
                Task task = Task.Run(() => _cloudService.AddCloudProcess(process));



                result.Success = true;
                result.Message = "OK";
                result.Description = "Document Creation is Succesful";
            }

            return result;
        }

        public List<DayResultState> GetDayResultState()
        {
            var states = _db.DayResultStates.ToList();

            return states;
        }

        public DayResultModel GetDayResult()
        {
            DayResultModel model = new DayResultModel();

            var location = _db.OurLocations.FirstOrDefault();
            DateTime date = location?.LocalDate ?? DateTime.Now.Date;

            var cash = _db.Cashes.FirstOrDefault(x => x.CashTypeId == 1 && x.IsMaster == true && x.LocationId == location.Id);

            var cashActions = _db.CashActions.Where(x => x.ActionDate == date && x.LocationId == location.Id && x.CashId == cash.Id).ToList();
            var bankActions = _db.BankActions.Where(x => x.ActionDate == date && x.LocationId == location.Id).ToList();

            var orders = _db.Vorders.Where(x => x.Date == date && x.OrderStatusId == 1 && x.LocationId == location.Id).ToList();
            var refunds = _db.Vorders.Where(x => x.Date == date && x.OrderStatusId == 3 && x.LocationId == location.Id).ToList();
            var orderRowSummaries = _db.VorderRowSummaries.Where(x => x.DateKey == date && x.LocationId == location.Id).ToList();

            var dayResult = _db.DayResults.FirstOrDefault(x => x.LocationId == location.Id && x.Date == date);
            var dayResultStates = _db.DayResultStates.ToList();

            if (dayResult == null)
            {
                var dayResultCloud = _cdb.DayResults.FirstOrDefault(x => x.LocationId == location.Id && x.Date == date);

                if (dayResultCloud == null)
                {
                    dayResult = new Data.Entities.DayResult()
                    {
                        LocationId = location.Id,
                        Date = date,
                        EnvironmentId = 4,
                        RecordDate = location.LocalDateTime,
                        StateId = 1,
                        Uid = Guid.NewGuid(),
                        RecordEmployeeId = 6070,
                        Description = string.Empty,
                    };

                    _db.DayResults.Add(dayResult);
                    _db.SaveChanges();

                    SyncProcess process = new SyncProcess()
                    {
                        DateCreate = DateTime.Now,
                        Entity = "DayResult",
                        Process = 1,
                        EntityId = dayResult.Id,
                        EntityUid = dayResult.Uid
                    };

                    _db.SyncProcesses.Add(process);
                    _db.SaveChanges();

                    Task task = Task.Run(() => _cloudService.AddCloudProcess(process));
                }
                else
                {
                    dayResult = new Data.Entities.DayResult()
                    {
                        LocationId = location.Id,
                        Date = date,
                        EnvironmentId = dayResultCloud.EnvironmentId,
                        RecordDate = dayResultCloud.RecordDate,
                        StateId = dayResultCloud.StateId,
                        Uid = dayResultCloud.Uid,
                        RecordEmployeeId = dayResultCloud.RecordEmployeeId,
                        Description = dayResultCloud.Description,
                    };

                    _db.DayResults.Add(dayResult);
                    _db.SaveChanges();
                }
            }


            model.DayResult = new Models.DayResult()
            {
                Date = dayResult.Date,
                Description = dayResult.Description,
                Id = dayResult.Id,
                EnvironmentId = dayResult.EnvironmentId,
                LocationId = dayResult.LocationId,
                PhotoFile = dayResult.PhotoFile,
                RecordDate = dayResult.RecordDate,
                RecordEmployeeId = dayResult.RecordEmployeeId,
                StateId = dayResult.StateId,
                Uid = dayResult.Uid.ToString(),
                UpdateDate = dayResult.UpdateDate,
                UpdateEmployeeId = dayResult.UpdateEmployeeId,
                StateName = dayResultStates.FirstOrDefault(x => x.Id == dayResult.StateId)?.StateName ?? "Unknown"
            };

            double cashTransferBalance = _db.CashActions.Where(x => x.ActionDate < date && x.LocationId == location.Id && x.CashId == cash.Id).ToList().Sum(x => x.Amount) ?? 0;
            double bankTransferBalance = _db.BankActions.Where(x => x.ActionDate < date && x.LocationId == location.Id).ToList().Sum(x => x.Amount) ?? 0;

            double cashAmount = cashActions.Sum(x => x.Amount) ?? 0;
            double expenseAmount = cashActions.Where(x => x.CashActionTypeId == 29).Sum(x => x.Amount) ?? 0;
            double laborAmount = cashActions.Where(x => x.CashActionTypeId == 31).Sum(x => x.Amount) ?? 0;
            double saleAmount = orders.Sum(x => x.TotalAmount) ?? 0;
            double saleCashAmount = orders.Where(x => x.PaymentType == 1 && x.OrderStatusId == 1).Sum(x => x.TotalAmount) ?? 0;
            double saleCreditAmount = orders.Where(x => x.PaymentType == 2 && x.OrderStatusId == 1).Sum(x => x.TotalAmount) ?? 0;
            double refundAmount = refunds.Sum(x => x.TotalAmount) ?? 0;
            double refundCashAmount = refunds.Where(x => x.PaymentType == 1).Sum(x => x.TotalAmount) ?? 0;
            double refundCreditAmount = refunds.Where(x => x.PaymentType == 2).Sum(x => x.TotalAmount) ?? 0;

            double creditAmount = bankActions.Sum(x => x.Amount) ?? 0;

            model.CashSummary = new Models.CashSummary()
            {
                CashBlockedAmount = cash.BlockedAmount ?? 0,
                CashAmount = cashAmount,
                CreditAmount = creditAmount,
                ExpenseAmount = expenseAmount,
                LaborAmount = laborAmount,
                SaleAmount = saleAmount,
                CashTransferBalance = cashTransferBalance,
                CreditTransferBalance = bankTransferBalance,
                CashBalance = cashAmount + cashTransferBalance,
                CreditBalance = creditAmount + bankTransferBalance,
                NetBalance = (cashAmount + cashTransferBalance) + (creditAmount + bankTransferBalance),
                saleCashAmount = saleCashAmount,
                saleCreditAmount = saleCreditAmount,
                RefundAmount = refundAmount,
                RefundCashAmount = refundCashAmount,
                RefundCreditAmount = refundCreditAmount
            };

            model.SaleSummaries = new List<TicketSaleSummary>();

            foreach (var rowSummary in orderRowSummaries)
            {
                model.SaleSummaries.Add(new TicketSaleSummary()
                {
                    PaymentType = rowSummary.MethodName,
                    SaleAmount = (double?)rowSummary.Total ?? 0,
                    SaleCount = rowSummary.Quantity ?? 0,
                    StatusName = rowSummary.StatusName,
                    TicketName = $"{rowSummary.TicketTypeName} {rowSummary.Duration} Mn.",
                    UnitPrice = (double?)rowSummary.Price ?? 0,
                    Unit = rowSummary.Duration ?? 0
                });
            }

            var locationSchedule = _db.LocationSchedules.FirstOrDefault(x => x.ScheduleDate == date && x.LocationId == location.Id);
            var locationShift = _db.LocationShifts.FirstOrDefault(x => x.ShiftDate == date && x.LocationId == location.Id);

            model.LocationShift = new LocationScheduleShift()
            {
                Id = locationSchedule?.Id ?? 0,
                ScheduleTime = $"{locationSchedule?.ScheduleStart.ToString("t") ?? "  :  "} - {locationSchedule?.ScheduleEnd?.ToString("t") ?? "  :  "}",
                ScheduleDuration = $"{locationSchedule?.ScheduleDuration?.ToString("t") ?? "  :  "}",
                ShiftTime = $"{locationShift?.ShiftStart.ToString("t") ?? "  :  "} - {locationShift?.ShiftFinish?.ToString("t") ?? "  :  "}",
                ShiftDuration = $"{locationShift?.ShiftDuration?.ToString("t") ?? "  :  "}",
                IsValid = locationSchedule != null && locationShift != null && locationShift?.ShiftFinish != null ? true : false
            };

            model.employeeShifts = new List<EmployeeScheduleShift>();

            var employeeSchedules = _db.EmployeeSchedules.Where(x => x.ScheduleDate == date && x.LocationId == location.Id).ToList();
            var employeeShifts = _db.EmployeeShifts.Where(x => x.ShiftDate == date && x.LocationId == location.Id).ToList();
            var employees = _db.Employees.ToList();

            List<int> empIds = employeeSchedules.Select(x => x.EmployeeId).ToList();

            foreach (var emp in empIds)
            {
                var employee = employees.FirstOrDefault(x => x.Id == emp);
                var employeeSchedule = employeeSchedules?.FirstOrDefault(x => x.EmployeeId == emp);
                var employeeShift = employeeShifts?.FirstOrDefault(x => x.EmployeeId == emp);

                if (employee != null)
                {
                    model.employeeShifts.Add(new EmployeeScheduleShift()
                    {
                        Id = emp,
                        Name = employee.FullName,
                        ScheduleTime = $"{employeeSchedule?.ShiftStart.ToString("t") ?? "  :  "} - {employeeSchedule?.ShiftEnd?.ToString("t") ?? "  :  "}",
                        ScheduleDuration = $"{employeeSchedule?.ShiftDuration?.ToString("t") ?? "  :  "}",
                        ShiftTime = $"{employeeShift?.ShiftStart.ToString("t") ?? "  :  "} - {employeeShift?.ShiftEnd?.ToString("t") ?? "  :  "}",
                        ShiftDuration = $"{employeeShift?.ShiftDuration?.ToString("t") ?? "  :  "}",
                        IsValid = employeeSchedule != null && employeeShift != null && employeeShift.ShiftEnd != null ? true : false
                    });
                }

            }


            return model;
        }

        public List<PosActionModel> GetActions()
        {
            List<PosActionModel> model = new List<PosActionModel>();

            var location = _db.OurLocations.FirstOrDefault();
            DateTime date = location?.LocalDate ?? DateTime.Now.Date;

            var actions = _db.Vactions.Where(x => x.ActionDate == date && x.LocationId == location.Id).OrderByDescending(x=> x.RecordDate).ToList();

            model = actions.Select(x=> new PosActionModel()
            {
                ActionDate= x.ActionDate,
                ActionTypeId= x.ActionTypeId,
                RecordDate= x.RecordDate,
                Amount= x.Amount,
                Currency= x.Currency,
                ProcessName= x.ProcessName,
                ProcessType = x.ProcessType ,
                ProcessUid = x.ProcessUid.ToString(),
                SourceName = x.SourceName
            }).ToList();

            return model;
        }

        public AppResult UpdateDayResult(string? FileName, int resultId, int stateId, string Description, int EmployeeId, DateTime DocDate, string FilePath)
        {
            var result = new AppResult() { Success = false, Message = string.Empty, Description = string.Empty };

            var dayResult = _db.DayResults.FirstOrDefault(x => x.Id == resultId);

            if (dayResult != null)
            {
                var location = _db.OurLocations.FirstOrDefault();

                dayResult.StateId = stateId;
                dayResult.Description = Description.Trim();
                dayResult.UpdateDate = location.LocalDateTime;
                dayResult.UpdateEmployeeId = EmployeeId;
                dayResult.PhotoFile = !string.IsNullOrEmpty(FileName) ? FileName : dayResult.PhotoFile;

                _db.SaveChanges();

                SyncProcess process = new SyncProcess()
                {
                    DateCreate = DateTime.Now,
                    Entity = "DayResult",
                    Process = 2,
                    EntityId = dayResult.Id,
                    EntityUid = dayResult.Uid,
                    FilePath = FilePath,
                };

                _db.SyncProcesses.Add(process);
                _db.SaveChanges(true);

                CloudService _cloudService = new CloudService();
                Task task = Task.Run(() => _cloudService.AddCloudProcess(process));



                result.Success = true;
                result.Message = "OK";
                result.Description = "DayResult is Updated";
            }
            else
            {
                result.Success = false;
                result.Message = "Not Found";
                result.Description = "DayResult was not Found";
            }

            return result;
        }

		public string GetCashDrawer()
		{

			var device = _db.DrawerDevices.Where(x => x.IsActive == true).OrderByDescending(x=> x.Id).FirstOrDefault();

            if (device != null)
            {
				return device.SerialNumber;

			}

			return "";

		}

	}
}
