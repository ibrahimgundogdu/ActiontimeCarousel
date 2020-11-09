using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class LocationServiceManager
    {
        private readonly ActionTimeEntities _db;
        private readonly Entity.Location _location;
        public LocationServiceManager(ActionTimeEntities db, LocationInfo location)
        {
            _db = db;
            _location = _db.Location.FirstOrDefault(x => x.LocationID == location.ID);
        }

        public SummaryControlModel GetLocationSummary(DateTime currentDate, LocationEmployee employee)
        {
            SummaryControlModel model = new SummaryControlModel();


            model.DayResult = _db.DayResult.FirstOrDefault(x => x.LocationID == _location.LocationID && x.Date == currentDate);

            if (model.DayResult == null)
            {
                var resultID = _db.AddDayResult(_location.LocationID, currentDate, 1, 3, employee.EmployeeID, "", LocationHelper.GetIPAddress()).FirstOrDefault();
                model.DayResult = _db.DayResult.FirstOrDefault(x => x.ID == resultID);
            }

            model.CurrentDayResult = _db.VDayResult.FirstOrDefault(x => x.LocationID == _location.LocationID && x.Date == currentDate);
            model.CashActions = _db.VCashActions.Where(x => x.LocationID == _location.LocationID && x.ActionDate == currentDate).ToList();
            model.BankActions = _db.VBankActions.Where(x => x.LocationID == _location.LocationID && x.ActionDate == currentDate).ToList();

            var trlCash = LocationHelper.GetCash(_location.LocationID, "TRL");
            var usdCash = LocationHelper.GetCash(_location.LocationID, "USD");
            var eurCash = LocationHelper.GetCash(_location.LocationID, "EUR");

            model.CurrentCash = LocationHelper.GetCash(_location.LocationID, _location.Currency);

            List<TotalModel> devirtotals = new List<TotalModel>();

            devirtotals.Add(new TotalModel()
            {
                Currency = "TRL",
                Total = _db.GetCashBalance(_location.LocationID, trlCash.ID, currentDate.AddDays(-1)).FirstOrDefault() ?? 0,
                CiroTotal = model.CurrentCash.Currency == "TRL" ? ((_db.GetCashBalance(_location.LocationID, trlCash.ID, currentDate.AddDays(-1)).FirstOrDefault() ?? 0) - model.CurrentCash.BlockedAmount) ?? 0 : 0
            });

            devirtotals.Add(new TotalModel()
            {
                Currency = "USD",
                Total = _db.GetCashBalance(_location.LocationID, usdCash.ID, currentDate.AddDays(-1)).FirstOrDefault() ?? 0,
                CiroTotal = model.CurrentCash.Currency == "USD" ? ((_db.GetCashBalance(_location.LocationID, usdCash.ID, currentDate.AddDays(-1)).FirstOrDefault() ?? 0) - model.CurrentCash.BlockedAmount) ?? 0 : 0
            });

            devirtotals.Add(new TotalModel()
            {
                Currency = "EUR",
                Total = _db.GetCashBalance(_location.LocationID, eurCash.ID, currentDate.AddDays(-1)).FirstOrDefault() ?? 0,
                CiroTotal = model.CurrentCash.Currency == "USD" ? ((_db.GetCashBalance(_location.LocationID, eurCash.ID, currentDate.AddDays(-1)).FirstOrDefault() ?? 0) - model.CurrentCash.BlockedAmount) ?? 0 : 0
            });

            model.DevirTotal = devirtotals;

            model.Result = new Result<DayResult>()
            {
                IsSuccess = true,
                Message = "",
                Data = null
            };

            model.ResultStates = _db.ResultState.Where(x => x.IsActive == true).ToList();

            return model;
        }

        public LocationBalance GetLocationSaleBalanceToday(DateTime DocumentDate)
        {

            LocationBalance balance = new LocationBalance() { Balance = 0, CashTotal = 0, CreditTotal = 0, Currency = _location.Currency, Date = DocumentDate.Date };

            var isbalance = _db.GetLocationSaleBalanceToday(_location.LocationID, DocumentDate).FirstOrDefault();

            if (isbalance != null)
            {
                balance.CashTotal = (float)isbalance.CashTotal;
                balance.CreditTotal = (float)isbalance.CreditTotal;
                balance.Balance = (float)isbalance.Balance;
                balance.Currency = isbalance.Currency;
                balance.Date = isbalance.CurrentDate ?? DocumentDate.Date;
                balance.CurrencySign = isbalance.CurrencySign;
            }

            return balance;
        }

        public List<LocationTicketSaleInfo> GetLocationTicketsToday(DateTime DocumentDate)
        {

            List<LocationTicketSaleInfo> list = new List<LocationTicketSaleInfo>();

            list = _db.GetLocationTicketsToday(_location.LocationID, DocumentDate).Select(x => new LocationTicketSaleInfo()
            {
                Currency = x.Currency,
                IsActive = x.IsActive.Value,
                Part = x.Part,
                PayMethodID = x.PaymethodID.Value,
                RecordDate = x.RecordDate.Value,
                RowID = x.RowID,
                SaleID = x.SaleID,
                StatusID = x.StatusID.Value,
                Total = (float)x.Total,
                Unit = x.Unit.Value,
                Uid = x.UID
            }).ToList();

            return list;
        }

        public List<EmployeeModel> GetLocationEmployeesToday()
        {

            List<EmployeeModel> resultlist = new List<EmployeeModel>();

             var schedules = _db.Schedule.Where(x => x.LocationID == _location.LocationID && x.ShiftDate == _location.LocalDate).ToList();

            List<int> empids = schedules.Select(x => x.EmployeeID.Value).Distinct().ToList();

            var shifts = _db.EmployeeShift.Where(x => x.LocationID == _location.LocationID && x.ShiftDate == _location.LocalDate && empids.Contains(x.EmployeeID.Value)).ToList();


            resultlist = _db.Employee.Where(x => empids.Contains(x.EmployeeID)).Select(x => new EmployeeModel()
            {
                ID = x.EmployeeID,
                FotoFile = x.FotoFile,
                FullName = x.FullName,
                Token = x.EmployeeUID
            }).ToList();

            foreach (var item in resultlist)
            {
                item.Schedule = schedules.Where(x => x.EmployeeID == item.ID).Select(x => new EmployeeScheduleModel()
                {
                    EmployeeID = item.ID,
                    ScheduleDate = x.ShiftDate.Value,
                    DateStart = x.ShiftDateStart.Value,
                    DateEnd = x.ShiftdateEnd,
                    Duration = (x.ShiftdateEnd - x.ShiftDateStart)


                }).FirstOrDefault();

                item.Shift = shifts.Where(x => x.EmployeeID == item.ID && x.IsWorkTime == true).Select(x => new EmployeeShiftLocationModel()
                {
                    EmployeeID = item.ID,
                    ShiftDate = x.ShiftDate.Value,
                    DateStart = x.ShiftDateStart.Value,
                    DateEnd = x.ShiftDateEnd,
                    Duration = (x.ShiftDateEnd - x.ShiftDateStart.Value)
                }).FirstOrDefault();

                item.Breaks = shifts.Where(x => x.EmployeeID == item.ID && x.IsBreakTime == true).Select(x => new EmployeeBreakModel()
                {
                    EmployeeID = item.ID,
                    BreaktDate = x.ShiftDate.Value,
                    DateStart = x.BreakDateStart.Value,
                    DateEnd = x.BreakDateEnd,
                    Duration = (x.BreakDateEnd - x.BreakDateStart.Value)

                }).ToList();

            }

            return resultlist;
        }

       

}
}