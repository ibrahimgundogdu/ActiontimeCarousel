using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ActionForce.PosLocation
{
    public class PosManager
    {
        public static string GetConnectionString()
        {
            string conn = "data source=37.1.145.98,50382;initial catalog=db293_ActionTimeDb;persist security info=True;user id=sa;password=n4q4n6U0FE!;MultipleActiveResultSets=True;";
            return conn;
        }
        public static string makeMD5(string strword)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(strword);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        public List<DataEmployee> GetLocationEmployeesToday(int LocationID)
        {

            List<DataEmployee> EmployeeList = new List<DataEmployee>();

            using (ActionTimeEntities _db = new ActionTimeEntities())
            {
                var location = _db.Location.FirstOrDefault(x => x.LocationID == LocationID);
                var employeeList = _db.GetLocationEmployees(LocationID).Where(x => x.Active == true).ToList();
                var schedules = _db.Schedule.Where(x => x.LocationID == LocationID && x.ShiftDate == location.LocalDate).ToList();

                EmployeeList = employeeList.Select(x => new DataEmployee()
                {
                    LocationID = LocationID,
                    EmployeeID = x.EmployeeID,
                    EmployeeFullname = x.FullName,
                    IsScheduled = schedules.Any(y => y.EmployeeID == x.EmployeeID),
                    PhotoFile = x.PhotoFile,
                    Token = x.EmployeeUID
                }).ToList();

            }
            return EmployeeList;
        }

        public static double GetStringToAmount(string amount)
        {
            double Amount = Convert.ToDouble(amount.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
            return Amount;
        }

        public static DateTime GetLocationScheduledDate(int LocationID, DateTime date)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                return db.GetLocationScheduledDate(LocationID, date).FirstOrDefault() ?? date.Date;
            }
        }

        public static long? GetDayResultID(int LocationID, DateTime date, int StateID, int EnvironmentID, int RecordEmployeeID, string Description, string RecordIP)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                return db.GetDayResultID(LocationID, date, StateID, EnvironmentID, RecordEmployeeID, Description, RecordIP).FirstOrDefault();
            }
        }

        public static IEnumerable<DifferentList> PublicInstancePropertiesEqual<T>(T self, T to, params string[] ignore) where T : class
        {
            List<DifferentList> loglist = new List<DifferentList>();

            if (self != null && to != null)
            {
                Type type = typeof(T);
                List<string> ignoreList = new List<string>(ignore);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (!ignoreList.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)) && pi.PropertyType.IsClass == false)
                        {
                            loglist.Add(new DifferentList() { FieldName = pi.Name, OldValue = selfValue?.ToString(), NewValue = toValue?.ToString(), TableName = type.Name });
                        }
                    }
                }

            }
            return loglist;
        }

        public static string[] getIgnorelist()
        {
            List<string> ignores = new List<string>() {
                " "
            };

            return ignores.ToArray();
        }

        public static void AddApplicationLog(string environment, string module, string processType, string processId, string controller, string action, IEnumerable<DifferentList> differents, bool isSuccess, string resultMessage, string errorMessage, DateTime recordDate, string recordEmployee, string recordIP, string recordDevice, object objdata)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                string data = Newtonsoft.Json.JsonConvert.SerializeObject(objdata);
                if (differents != null)
                {
                    if (differents.Count() > 0)
                    {
                        foreach (var item in differents)
                        {
                            db.AddApplicationLog(environment, module, processType, processId, controller, action, item.TableName, item.FieldName, item.OldValue, item.NewValue, isSuccess, resultMessage, errorMessage, recordDate, recordEmployee, recordIP, recordDevice, data);
                        }
                    }
                    else
                    {
                        db.AddApplicationLog(environment, module, processType, processId, controller, action, string.Empty, string.Empty, string.Empty, string.Empty, isSuccess, resultMessage, errorMessage, recordDate, recordEmployee, recordIP, recordDevice, data);

                    }

                }
                else
                {
                    db.AddApplicationLog(environment, module, processType, processId, controller, action, string.Empty, string.Empty, string.Empty, string.Empty, isSuccess, resultMessage, errorMessage, recordDate, recordEmployee, recordIP, recordDevice, data);

                }
            }
        }

        public static Cash GetCash(int locationID, string currency)
        {
            Cash cash = new Cash();

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                cash = db.Cash.FirstOrDefault(x => x.LocationID == locationID && x.Currency == currency);
                if (cash != null)
                {
                    return cash;
                }
                else
                {
                    return CreateCash(locationID, currency);
                }
            }

        }

        public static Cash CreateCash(int locationID, string currency)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                var location = db.Location.FirstOrDefault(x => x.LocationID == locationID);

                Cash newcash = new Cash();

                if (currency == "USD")
                {
                    newcash.CashName = $"Location Cash";
                    newcash.BlockedAmount = location.Currency == currency ? 100 : 0;
                }
                else if (currency == "TRL")
                {
                    newcash.CashName = $"Lokasyon Kasası";
                    newcash.BlockedAmount = location.Currency == currency ? 280 : 0;
                }
                else
                {
                    newcash.CashName = $"Location Cash";
                    newcash.BlockedAmount = 0;
                }

                newcash.Currency = currency;
                newcash.IsActive = true;
                newcash.LocationID = locationID;
                newcash.SortBy = "01";
                newcash.IsMaster = location.Currency == currency ? true : false;

                db.Cash.Add(newcash);
                db.SaveChanges();

                return newcash;
            }
        }

        private readonly ActionTimeEntities _db;
        public PosManager()
        {
            _db = new ActionTimeEntities();
        }

        public SummaryControlModel GetLocationSummary(DateTime currentDate, CurrentEmployee employee, Location _location)
        {
            SummaryControlModel model = new SummaryControlModel();

            model.DayResult = _db.DayResult.FirstOrDefault(x => x.LocationID == _location.LocationID && x.Date == currentDate);

            if (model.DayResult == null)
            {
                var resultID = _db.AddDayResult(_location.LocationID, currentDate, 1, 3, employee.EmployeeID, "", GetIPAddress()).FirstOrDefault();
                model.DayResult = _db.DayResult.FirstOrDefault(x => x.ID == resultID);
            }

            model.CurrentDayResult = _db.VDayResult.FirstOrDefault(x => x.LocationID == _location.LocationID && x.Date == currentDate);
            model.CashActions = _db.VCashActions.Where(x => x.LocationID == _location.LocationID && x.ActionDate == currentDate).ToList();
            model.BankActions = _db.VBankActions.Where(x => x.LocationID == _location.LocationID && x.ActionDate == currentDate).ToList();

            var trlCash = GetCash(_location.LocationID, "TRL");
            var usdCash = GetCash(_location.LocationID, "USD");
            var eurCash = GetCash(_location.LocationID, "EUR");

            model.CurrentCash = GetCash(_location.LocationID, _location.Currency);

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

        public LocationBalance GetLocationSaleBalanceToday(DateTime DocumentDate, Location _location)
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

        public List<EmployeeModel> GetLocationEmployeeModelsToday(Location _location)
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

        public List<LocationTicketSaleInfo> GetLocationTicketsToday(DateTime DocumentDate, Location _location)
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
    }
}