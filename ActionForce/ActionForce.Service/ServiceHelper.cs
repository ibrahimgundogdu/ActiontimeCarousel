using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ActionForce.Entity;
using System.Globalization;
using ActionForce.Integration;

namespace ActionForce.Service
{

    public class ServiceHelper
    {

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

        public static Exchange GetExchange(DateTime date)
        {
            DateTime processDate = date.Date;
            DateTime filterDate = date.Date;

            if (DateTime.UtcNow.Hour >= 12 && DateTime.UtcNow.Minute > 0)
            {
                processDate = date.AddDays(1).Date;
            }


            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                var exchange = db.Exchange.FirstOrDefault(x => x.Date == processDate);

                if (exchange != null)
                {
                    return exchange;
                }
                else
                {
                    var datekey = db.DateList.FirstOrDefault(x => x.DateKey == processDate);

                    if (datekey.DayName == "Saturday" || datekey.DayName == "Sunday")
                    {
                        filterDate = db.DateList.FirstOrDefault(x => x.Year == datekey.Year && x.WeekNumber == datekey.WeekNumber && x.DayName == "Friday").DateKey;
                    }

                    TCMBClient tcmbClient = new TCMBClient();

                    string dateparam = filterDate.ToString("dd-MM-yyyy");

                    var newExchange = tcmbClient.GetExchangeToday(dateparam);

                    if (newExchange != null)
                    {
                        Exchange newexchange = new Exchange();

                        var item = newExchange.items.FirstOrDefault();

                        if (item != null && item.TP_DK_EUR_A != null && item.TP_DK_USD_A != null)
                        {
                            newexchange.Date = processDate;
                            newexchange.EURA = Convert.ToDouble(item.TP_DK_EUR_A, CultureInfo.InvariantCulture);
                            newexchange.EURS = Convert.ToDouble(item.TP_DK_EUR_S, CultureInfo.InvariantCulture);
                            newexchange.USDA = Convert.ToDouble(item.TP_DK_USD_A, CultureInfo.InvariantCulture);
                            newexchange.USDS = Convert.ToDouble(item.TP_DK_USD_S, CultureInfo.InvariantCulture);

                            db.Exchange.Add(newexchange);
                            db.SaveChanges();

                            return newexchange;
                        }
                        else
                        {
                            return db.Exchange.OrderByDescending(x => x.Date).FirstOrDefault();
                        }

                    }
                    else
                    {
                        return db.Exchange.OrderByDescending(x => x.Date).FirstOrDefault();
                    }

                }
            }

        }


        public static string GetDocumentNumber(int ourCompanyID, string Prefix)
        {
            string documentNumber = string.Empty;

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                documentNumber = db.GetDocumentNumber(ourCompanyID, Prefix).FirstOrDefault();
            }

            return documentNumber;
        }

        public static void AddCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.AddCashAction(CashID, LocationID, EmployeeID, CashActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
            }
        }

        public static IEnumerable<AuditTrail> PublicInstancePropertiesEqual<T>(T self, T to, params string[] ignore) where T : class
        {
            List<AuditTrail> loglist = new List<AuditTrail>();

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
                            loglist.Add(new AuditTrail() { FieldName = pi.Name, OldValue = selfValue?.ToString(), NewValue = toValue?.ToString(), TableName = type.Name });
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

        public static void AddApplicationLog(string environment, string module, string processType, string processId, string controller, string action, IEnumerable<AuditTrail> differents, bool isSuccess, string resultMessage, string errorMessage, DateTime recordDate, string recordEmployee, string recordIP, string recordDevice, object objdata)
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




















        //public static IEnumerable<FromAccountModel> GetFromList(int ourCompanyID)
        //{

        //    List<FromAccountModel> fromList = new List<FromAccountModel>();


        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        fromList = db.GetFromList(ourCompanyID).Select(x => new FromAccountModel()
        //        {
        //            ID = x.ID,
        //            Code = x.Code,
        //            Name = x.Name,
        //            Prefix = x.Prefix
        //        }).ToList();
        //    }


        //    return fromList;
        //}



        //public static IEnumerable<Currency> GetCurrency()
        //{
        //    List<Currency> currList = new List<Currency>();
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        currList = db.Currency.ToList();
        //    }
        //    return currList;
        //}

        //public static void AddCashes(int locationID)
        //{
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        var currencyList = db.Currency.ToList();

        //        if (currencyList != null && currencyList.Count > 0)
        //        {
        //            foreach (var item in currencyList)
        //            {
        //                CreateCash(locationID, item.Code);
        //            }
        //        }
        //    }
        //}



        //public static int GetTimeZone(int locationID)
        //{
        //    int timezone = 3;

        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        timezone = db.Location.FirstOrDefault(x => x.LocationID == locationID)?.Timezone ?? 3;
        //        return timezone;
        //    }

        //}




        //public static void AddCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        //{
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        db.AddCashAction(CashID, LocationID, EmployeeID, CashActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
        //    }
        //}

        //public static void AddBankAction(int? LocationID, int? EmployeeID, int? BankAccountID, int? PosID, int? BankActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        //{
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        db.AddBankAction(LocationID, EmployeeID, BankAccountID, PosID, BankActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
        //    }
        //}

        //public static void AddEmployeeAction(int? EmployeeID, int? LocationID, int? ActionTypeID, string ProcessName, long? ProcessID, DateTime? ProcessDate, string ProcessDetail, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? SalaryTypeID, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID, string DocumentNumber, int SalaryCategoryID)
        //{
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        db.AddEmployeeAction(EmployeeID, LocationID, ActionTypeID, ProcessName, ProcessID, ProcessDate, ProcessDetail, Direction, Collection, Payment, Currency, Latitude, Longitude, SalaryTypeID, RecordEmployeeID, RecordDate, ProcessUID, DocumentNumber, SalaryCategoryID);
        //    }
        //}

        //public static void AddCustomerAction(int? CustomerID, int? LocationID, int? EmployeeID, int? ActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        //{
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        db.AddCustomerAction(CustomerID, LocationID, EmployeeID, ActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
        //    }
        //}

        //public static void UpdateCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, int? UpdateEmployeeID, DateTime? UpdateDate)
        //{
        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        db.UpdateCashAction(CashID, LocationID, EmployeeID, CashActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, UpdateEmployeeID, UpdateDate);
        //    }
        //}

        //public static Result<DayResult> AddItemsToResultEnvelope(long? id)
        //{

        //    // ID Module  Category ItemName
        //    //1   Cash NULL    Kasa Tahsilatı
        //    //2   Cash NULL    Kasa Satışı
        //    //3   Cash NULL    Döviz Satışı
        //    //4   Cash NULL    Kasa Ödemesi
        //    //5   Cash NULL    Kasa Satış İadesi
        //    //6   Cash NULL    Kasadan Masraf Ödemesi
        //    //7   Cash NULL    Havale / EFT
        //    //8   Cash NULL    Maaş Hakedişi
        //    //9   Cash NULL    Maaş / Avans Ödemesi
        //    //10  Bank NULL    Pos(Kredi Kartı)  Satışı
        //    //11  Bank NULL    Pos(Kredi Kartı)  Satış İptali
        //    //12  Bank NULL    Pos(Kredi Kartı)  Satış İadesi
        //    //13  CashRecorder NULL    Yazarkasa Fişi
        //    //14  File NULL    Dosyalar
        //    //15  Price Check

        //    Result<DayResult> result = new Result<DayResult>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty
        //    };

        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        ResultControlModel model = new ResultControlModel();

        //        var dayresult = db.DayResult.FirstOrDefault(x => x.ID == id);

        //        if (dayresult != null)
        //        {
        //            List<DayResultItemList> itemlist = new List<DayResultItemList>();

        //            var items = db.DayResultItems.ToList();
        //            var location = db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);

        //            if (location.OurCompanyID == 2)  // ülke türkiye ise
        //            {
        //                result.IsSuccess = true;

        //                // 01 Kasa Tahsilatı ekle
        //                var exchange = GetExchange(dayresult.Date);

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "USD",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.USDA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //eur
        //                itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "EUR",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.EURA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 02 Kasa Satışı ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "USD",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.USDA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //eur
        //                itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "EUR",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.EURA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 03 Kasa Döviz Satışı ekle

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "USD",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.USDA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "EUR",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.EURA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 04 Kasa Ödemesi Ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "USD",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.USDA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //eur
        //                itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "EUR",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.EURA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 05 Kasa Satış İadesi Ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "USD",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.USDA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //eur
        //                itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "EUR",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = exchange.EURA,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 06 Kasadan Masraf Ödemesi Ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 6).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 07 Kasadan Havale EFT çıkma Ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 7).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 1,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());




        //                // 08 Maaş Hakedişi işle Maaş Avans Ödemesi Ekle

        //                var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

        //                if (locschedule != null)
        //                {
        //                    var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
        //                    List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();

        //                    var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
        //                    var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

        //                    foreach (var emp in empids)
        //                    {
        //                        var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID, model.Authentication);

        //                        //var employeeschedule = empschedules.FirstOrDefault(x => x.EmployeeID == emp);
        //                        //var employeeshift = empshifts.FirstOrDefault(x => x.EmployeeID == emp);
        //                        //var hourprice = empunits.Where(x => x.EmployeeID == emp && x.Hourly > 0 && x.DateStart <= dayresult.Date).OrderByDescending(x => x.DateStart).FirstOrDefault();

        //                        //double? durationhour = 0;
        //                        //double? unithour = hourprice?.Hourly ?? 0;
        //                        //TimeSpan? duration = null;

        //                        //if (employeeschedule != null && employeeshift != null)
        //                        //{
        //                        //    DateTime? starttime = employeeschedule.ShiftDateStart;
        //                        //    if (employeeshift.ShiftDateStart > starttime)
        //                        //    {
        //                        //        starttime = employeeshift.ShiftDateStart;
        //                        //    }

        //                        //    DateTime? finishtime = employeeschedule.ShiftdateEnd;
        //                        //    if (employeeshift.ShiftDateEnd < finishtime)
        //                        //    {
        //                        //        finishtime = employeeshift.ShiftDateEnd;
        //                        //    }

        //                        //    if (finishtime != null && starttime != null)
        //                        //    {
        //                        //        duration = (finishtime - starttime).Value;
        //                        //        double? durationminute = (finishtime - starttime).Value.TotalMinutes;
        //                        //        durationhour = (durationminute / 60);
        //                        //    }


        //                        //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = (durationhour * unithour),
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = (durationhour * unithour),
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        SystemHourQuantity = durationhour,
        //                        //        UnitHourPrice = unithour,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress(),
        //                        //        Duration = duration,
        //                        //        SystemDuration = duration,
        //                        //        HourQuantity = durationhour

        //                        //    }).FirstOrDefault());

        //                        //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = 0,
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = 0,
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress()

        //                        //    }).FirstOrDefault());

        //                        //}
        //                        //else
        //                        //{
        //                        //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = 0,
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = 0,
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        SystemHourQuantity = 0,
        //                        //        UnitHourPrice = unithour,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress()

        //                        //    }).FirstOrDefault());

        //                        //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = 0,
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = 0,
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress()

        //                        //    }).FirstOrDefault());
        //                        //}
        //                    }


        //                }
        //                else
        //                {
        //                    result.Message = "Lokasyon takvimi tanımlanmamış";
        //                }

        //                // 10 POS Kredi Kartı Satışı Ekle

        //                itemlist.Add(items.Where(x => x.ID == 10).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 8,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 11 POS Kredi Kartı Satışı İptali Ekle

        //                itemlist.Add(items.Where(x => x.ID == 11).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 8,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 12 POS Kredi Kartı Satışı İadesi Ekle

        //                itemlist.Add(items.Where(x => x.ID == 12).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 8,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                // 13 Yazarkasa satışı fişi Ekle

        //                itemlist.Add(items.Where(x => x.ID == 13).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress(),
        //                    SlipNumber = string.Empty,
        //                    SlipDate = dayresult.Date.AddHours(22),
        //                    SlipTotalAmount = 0

        //                }).FirstOrDefault());




        //            }
        //            else if (location.OurCompanyID == 1)  // ülke amerika ise
        //            {
        //                result.IsSuccess = true;

        //                // 01 Kasa Tahsilatı ekle
        //                var exchange = GetExchange(dayresult.Date);

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 02 Kasa Satışı ekle

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 03 Kasa Döviz Satışı ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "TRL",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = (1 / exchange.USDA),
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                //eur
        //                itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = "EUR",
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = (1 / exchange.EURA),
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 04 Kasa Ödemesi Ekle

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 05 Kasa Satış İadesi Ekle

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 06 Kasadan Masraf Ödemesi Ekle

        //                //trl
        //                itemlist.Add(items.Where(x => x.ID == 6).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 07 Kasadan Havale EFT çıkma Ekle

        //                //usd
        //                itemlist.Add(items.Where(x => x.ID == 7).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 4,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());




        //                // 08 Maaş Hakedişi işle Maaş Avans Ödemesi Ekle

        //                var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

        //                if (locschedule != null)
        //                {
        //                    var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
        //                    List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();


        //                    var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
        //                    var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

        //                    foreach (var emp in empids)
        //                    {

        //                        var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID, model.Authentication);

        //                        //var employeeschedule = empschedules.FirstOrDefault(x => x.EmployeeID == emp);
        //                        //var employeeshift = empshifts.FirstOrDefault(x => x.EmployeeID == emp);
        //                        //var hourprice = empunits.Where(x => x.EmployeeID == emp && x.Hourly > 0 && x.DateStart <= dayresult.Date).OrderByDescending(x => x.DateStart).FirstOrDefault();

        //                        //double? durationhour = 0;
        //                        //double? unithour = hourprice?.Hourly ?? 0;
        //                        //TimeSpan? duration = null;

        //                        //if (employeeschedule != null && employeeshift != null)
        //                        //{
        //                        //    DateTime? starttime = employeeschedule.ShiftDateStart;
        //                        //    if (employeeshift.ShiftDateStart > starttime)
        //                        //    {
        //                        //        starttime = employeeshift.ShiftDateStart;
        //                        //    }

        //                        //    DateTime? finishtime = employeeschedule.ShiftdateEnd;
        //                        //    if (employeeshift.ShiftDateEnd < finishtime)
        //                        //    {
        //                        //        finishtime = employeeshift.ShiftDateEnd;
        //                        //    }

        //                        //    if (finishtime != null && starttime != null)
        //                        //    {
        //                        //        duration = (finishtime - starttime).Value;
        //                        //        double? durationminute = (finishtime - starttime).Value.TotalMinutes;
        //                        //        durationhour = (durationminute / 60);
        //                        //    }

        //                        //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = (durationhour * hourprice.Hourly),
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = (durationhour * hourprice.Hourly),
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        SystemHourQuantity = durationhour,
        //                        //        UnitHourPrice = hourprice.Hourly,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress(),
        //                        //        Duration = duration,
        //                        //        SystemDuration = duration,
        //                        //        HourQuantity = durationhour

        //                        //    }).FirstOrDefault());

        //                        //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = 0,
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = 0,
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress()

        //                        //    }).FirstOrDefault());

        //                        //}
        //                        //else
        //                        //{
        //                        //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = 0,
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = 0,
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        SystemHourQuantity = 0,
        //                        //        UnitHourPrice = hourprice.Hourly,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress()

        //                        //    }).FirstOrDefault());

        //                        //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
        //                        //    {
        //                        //        Amount = 0,
        //                        //        Category = x.Category,
        //                        //        Currency = location.Currency,
        //                        //        ResultID = id,
        //                        //        ResultItemID = x.ID,
        //                        //        Quantity = 0,
        //                        //        SystemQuantity = 0,
        //                        //        Exchange = 1,
        //                        //        SystemAmount = 0,
        //                        //        LocationID = location.LocationID,
        //                        //        Date = dayresult.Date,
        //                        //        EmployeeID = emp,
        //                        //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                        //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                        //        RecordIP = GetIPAddress()

        //                        //    }).FirstOrDefault());
        //                        //}
        //                    }


        //                }
        //                else
        //                {
        //                    result.Message = "Lokasyon takvimi tanımlanmamış";
        //                }

        //                // 10 POS Kredi Kartı Satışı Ekle

        //                itemlist.Add(items.Where(x => x.ID == 10).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 7,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 11 POS Kredi Kartı Satışı İptali Ekle

        //                itemlist.Add(items.Where(x => x.ID == 11).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 7,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());


        //                // 12 POS Kredi Kartı Satışı İadesi Ekle

        //                itemlist.Add(items.Where(x => x.ID == 12).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    BankAccountID = 7,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress()

        //                }).FirstOrDefault());

        //                // 13 Yazarkasa satışı fişi Ekle

        //                itemlist.Add(items.Where(x => x.ID == 13).Select(x => new DayResultItemList()
        //                {
        //                    Amount = 0,
        //                    Category = x.Category,
        //                    Currency = location.Currency,
        //                    ResultID = id,
        //                    ResultItemID = x.ID,
        //                    Quantity = 0,
        //                    SystemQuantity = 0,
        //                    Exchange = 1,
        //                    SystemAmount = 0,
        //                    LocationID = location.LocationID,
        //                    Date = dayresult.Date,
        //                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                    RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
        //                    RecordIP = GetIPAddress(),
        //                    SlipNumber = string.Empty,
        //                    SlipDate = dayresult.Date.AddHours(22),
        //                    SlipTotalAmount = 0

        //                }).FirstOrDefault());

        //            }

        //            // 15 fiyat kontrol bölümü

        //            db.DayResultItemList.AddRange(itemlist);
        //            db.SaveChanges();

        //            CheckDayResultPriceList(dayresult.ID);
        //        }
        //    }
        //    return result;

        //}

        //public static string BankReferenceCode(int OurCompanyID)
        //{
        //    string rndNumber = string.Empty;

        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        List<string> numberlist = db.DocumentBankTransfer.Select(x => x.ReferenceCode).ToList();

        //        rndNumber = OurCompanyID.ToString() + DateTime.Now.ToString("yy");

        //        Random rnd = new Random();

        //        for (int i = 1; i < 6; i++)
        //        {
        //            rndNumber += rnd.Next(0, 9).ToString();
        //        }

        //        if (!numberlist.Contains(rndNumber))
        //        {
        //            return rndNumber;
        //        }
        //        else
        //        {
        //            return BankReferenceCode(OurCompanyID);
        //        }
        //    }
        //}

        //public static bool CalculateSalaryEarn(long? resultid, int employeeid, DateTime? date, int? locationid, AuthenticationModel authentication)
        //{
        //    bool issuccess = false;

        //    if (employeeid > 0 && (resultid > 0 || date != null || locationid > 0))
        //    {

        //        using (ActionTimeEntities db = new ActionTimeEntities())
        //        {
        //            var dayresult = db.VDayResult.FirstOrDefault(x => x.ID == resultid.Value);

        //            if (dayresult == null)
        //            {
        //                date = date.Value.Date;
        //                dayresult = db.VDayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
        //            }


        //            if (dayresult != null)
        //            {
        //                List<DayResultItemList> itemlist = new List<DayResultItemList>();

        //                var items = db.DayResultItems.ToList();

        //                var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date);
        //                var location = db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);

        //                if (locschedule != null)
        //                {
        //                    var employeeschedule = db.Schedule.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date && x.EmployeeID == employeeid);
        //                    var employeeshift = db.EmployeeShift.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date && x.EmployeeID == employeeid);

        //                    double? durationhour = 0;

        //                    TimeSpan? duration = null;

        //                    if (employeeschedule != null && employeeshift != null)
        //                    {
        //                        DateTime? starttime = employeeschedule.ShiftDateStart;
        //                        if (employeeshift.ShiftDateStart > starttime)
        //                        {
        //                            starttime = employeeshift.ShiftDateStart;
        //                        }

        //                        DateTime? finishtime = employeeschedule.ShiftdateEnd;
        //                        if (employeeshift.ShiftDateEnd < finishtime)
        //                        {
        //                            finishtime = employeeshift.ShiftDateEnd;
        //                        }

        //                        if (finishtime != null && starttime != null)
        //                        {
        //                            duration = (finishtime - starttime).Value;
        //                            double? durationminute = (finishtime - starttime).Value.TotalMinutes;
        //                            durationhour = (durationminute / 60);
        //                        }

        //                        // varmı yokmu
        //                        var existsitem = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 8);
        //                        if (existsitem != null)
        //                        {
        //                            db.DayResultItemList.Remove(existsitem);
        //                            db.SaveChanges();
        //                        }

        //                        itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
        //                        {
        //                            Amount = 0,
        //                            Category = x.Category,
        //                            Currency = location.Currency,
        //                            ResultID = dayresult.ID,
        //                            ResultItemID = x.ID,
        //                            Quantity = 0,
        //                            SystemQuantity = 0,
        //                            Exchange = 1,
        //                            SystemAmount = 0,
        //                            LocationID = location.LocationID,
        //                            Date = dayresult.Date,
        //                            EmployeeID = employeeid,
        //                            SystemHourQuantity = durationhour,
        //                            UnitHourPrice = 0,
        //                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                            RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
        //                            RecordIP = GetIPAddress(),
        //                            Duration = duration,
        //                            SystemDuration = duration,
        //                            HourQuantity = durationhour

        //                        }).FirstOrDefault());

        //                        var existsitem9 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 9);
        //                        if (existsitem9 != null)
        //                        {
        //                            db.DayResultItemList.Remove(existsitem9);
        //                            db.SaveChanges();
        //                        }

        //                        itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
        //                        {
        //                            Amount = 0,
        //                            Category = x.Category,
        //                            Currency = location.Currency,
        //                            ResultID = dayresult.ID,
        //                            ResultItemID = x.ID,
        //                            Quantity = 0,
        //                            SystemQuantity = 0,
        //                            Exchange = 1,
        //                            SystemAmount = 0,
        //                            LocationID = location.LocationID,
        //                            Date = dayresult.Date,
        //                            EmployeeID = employeeid,
        //                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                            RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
        //                            RecordIP = GetIPAddress()

        //                        }).FirstOrDefault());

        //                        // maaş hakedişi dosya ve hareket olarak ekle
        //                        DocumentManager documentManager = new DocumentManager();

        //                        var existssalaryearn = db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ActionTypeID == 32 && x.IsActive == true);

        //                        if (existssalaryearn != null)
        //                        {

        //                            SalaryEarn earn = new SalaryEarn();

        //                            earn.ActionTypeID = 32;
        //                            earn.ActionTypeName = "Ücret Hakediş";
        //                            earn.Currency = location.Currency;
        //                            earn.DocumentDate = dayresult.Date;
        //                            earn.EmployeeID = employeeid;
        //                            earn.EnvironmentID = 2;
        //                            earn.LocationID = location.LocationID;
        //                            earn.OurCompanyID = location.OurCompanyID;
        //                            earn.QuantityHour = durationhour.Value;
        //                            earn.ResultID = dayresult.ID;
        //                            earn.TimeZone = location.Timezone;
        //                            earn.UID = existssalaryearn.UID.Value;
        //                            earn.Description = existssalaryearn.Description;

        //                            var res = documentManager.EditSalaryEarn(earn, authentication);  // log zaten var.

        //                        }
        //                        else
        //                        {

        //                            SalaryEarn earn = new SalaryEarn();

        //                            earn.ActionTypeID = 32;
        //                            earn.ActionTypeName = "Ücret Hakediş";
        //                            earn.Currency = location.Currency;
        //                            earn.DocumentDate = dayresult.Date;
        //                            earn.EmployeeID = employeeid;
        //                            earn.EnvironmentID = 2;
        //                            earn.LocationID = location.LocationID;
        //                            earn.OurCompanyID = location.OurCompanyID;
        //                            earn.QuantityHour = durationhour.Value;
        //                            earn.ResultID = dayresult.ID;
        //                            earn.TimeZone = location.Timezone;
        //                            earn.UID = Guid.NewGuid();

        //                            var res = documentManager.AddSalaryEarn(earn, authentication);  // log zaten var.
        //                        }

        //                    }
        //                    else
        //                    {

        //                        var existsitem8 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 8);
        //                        if (existsitem8 != null)
        //                        {
        //                            db.DayResultItemList.Remove(existsitem8);
        //                            db.SaveChanges();
        //                        }

        //                        var existsitem9 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 9);
        //                        if (existsitem9 != null)
        //                        {
        //                            db.DayResultItemList.Remove(existsitem9);
        //                            db.SaveChanges();
        //                        }

        //                        itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
        //                        {
        //                            Amount = 0,
        //                            Category = x.Category,
        //                            Currency = location.Currency,
        //                            ResultID = dayresult.ID,
        //                            ResultItemID = x.ID,
        //                            Quantity = 0,
        //                            SystemQuantity = 0,
        //                            Exchange = 1,
        //                            SystemAmount = 0,
        //                            LocationID = location.LocationID,
        //                            Date = dayresult.Date,
        //                            EmployeeID = employeeid,
        //                            SystemHourQuantity = 0,
        //                            UnitHourPrice = 0,
        //                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                            RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
        //                            RecordIP = GetIPAddress()

        //                        }).FirstOrDefault());

        //                        itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
        //                        {
        //                            Amount = 0,
        //                            Category = x.Category,
        //                            Currency = location.Currency,
        //                            ResultID = dayresult.ID,
        //                            ResultItemID = x.ID,
        //                            Quantity = 0,
        //                            SystemQuantity = 0,
        //                            Exchange = 1,
        //                            SystemAmount = 0,
        //                            LocationID = location.LocationID,
        //                            Date = dayresult.Date,
        //                            EmployeeID = employeeid,
        //                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
        //                            RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
        //                            RecordIP = GetIPAddress()

        //                        }).FirstOrDefault());


        //                    }
        //                }

        //                db.DayResultItemList.AddRange(itemlist);
        //                db.SaveChanges();
        //            }
        //        }
        //    }

        //    return issuccess;
        //}

        //public static bool CheckSalaryEarn(DateTime? date, int? locationid, AuthenticationModel authentication)
        //{
        //    bool issuccess = false;

        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        var location = db.Location.FirstOrDefault(x => x.LocationID == locationid);
        //        var dayresult = db.DayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
        //        var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

        //        if (locschedule != null)
        //        {
        //            var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
        //            List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();


        //            var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
        //            var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

        //            foreach (var emp in empids)
        //            {
        //                var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID, authentication);
        //            }

        //            issuccess = true;
        //        }
        //    }

        //    return issuccess;
        //}

        //public static double CalculatePermitDuration(DateTime datestart, DateTime dateend, int employeeid, int locationid)
        //{
        //    double minuteDuration = 0;

        //    if (employeeid > 0 && locationid > 0 && datestart != null && dateend != null)
        //    {
        //        DateTime date = datestart.Date;

        //        using (ActionTimeEntities db = new ActionTimeEntities())
        //        {
        //            var employeeschedule = db.Schedule.FirstOrDefault(x => x.EmployeeID == employeeid && x.LocationID == locationid && x.ShiftDate == date);
        //            var employeeshift = db.EmployeeShift.FirstOrDefault(x => x.EmployeeID == employeeid && x.LocationID == locationid && x.ShiftDate == date && x.IsWorkTime == true);

        //            if (employeeschedule != null)
        //            {
        //                DateTime _startdate = employeeschedule.ShiftDateStart.Value;
        //                DateTime _endate = employeeschedule.ShiftdateEnd.Value;

        //                if (employeeshift != null)
        //                {
        //                    if (employeeshift.ShiftDateStart != null && employeeshift.ShiftDateStart > employeeschedule.ShiftDateStart)
        //                    {
        //                        _startdate = employeeshift.ShiftDateStart.Value;
        //                    }

        //                    if (employeeshift.ShiftDateEnd != null && employeeshift.ShiftDateEnd < employeeschedule.ShiftdateEnd)
        //                    {
        //                        _endate = employeeshift.ShiftDateEnd.Value;
        //                    }
        //                }

        //                datestart = datestart >= _startdate ? datestart : _startdate;
        //                dateend = dateend <= _endate ? dateend : _endate;

        //                minuteDuration = (int)(dateend - datestart).TotalMinutes;
        //            }
        //        }
        //    }

        //    return minuteDuration;
        //}

        //public static void Compute(int? WeekYear, int? WeekNumber, int? LocationID)
        //{
        //    RevenueControlModel model = new RevenueControlModel();

        //    model.WeekYear = WeekYear ?? 0;
        //    model.WeekNumber = WeekNumber ?? 0;

        //    if (WeekYear > 0 && WeekNumber > 0)
        //    {
        //        using (ActionTimeEntities db = new ActionTimeEntities())
        //        {
        //            if (LocationID > 0)
        //            {
        //                var res = db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, LocationID);
        //            }
        //            else
        //            {
        //                model.Locations = db.Location.ToList();

        //                foreach (var location in model.Locations)
        //                {
        //                    var res = db.ComputeLocationWeekRevenue(model.WeekNumber, model.WeekYear, location.LocationID);
        //                }
        //            }
        //        }
        //    }
        //}

        //public static bool ComputeSalaryEarn(int employeeid, DateTime? date, int? locationid, AuthenticationModel authentication)
        //{
        //    bool issuccess = false;

        //    if (employeeid > 0 && date != null && locationid > 0)
        //    {

        //        using (ActionTimeEntities db = new ActionTimeEntities())
        //        {
        //            var dayresult = db.DayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
        //            var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == locationid && x.ShiftDate == date);
        //            var location = db.Location.FirstOrDefault(x => x.LocationID == locationid);

        //            if (locschedule != null)
        //            {
        //                var employeeschedule = db.Schedule.FirstOrDefault(x => x.LocationID == locationid && x.ShiftDate == date && x.EmployeeID == employeeid);
        //                var employeeshift = db.EmployeeShift.FirstOrDefault(x => x.LocationID == locationid && x.ShiftDate == date && x.EmployeeID == employeeid);


        //                double? durationhour = 0;

        //                TimeSpan? duration = null;

        //                if (employeeschedule != null && employeeshift != null)
        //                {
        //                    DateTime? starttime = employeeschedule.ShiftDateStart;
        //                    if (employeeshift.ShiftDateStart > starttime)
        //                    {
        //                        starttime = employeeshift.ShiftDateStart;
        //                    }

        //                    DateTime? finishtime = employeeschedule.ShiftdateEnd;
        //                    if (employeeshift.ShiftDateEnd < finishtime)
        //                    {
        //                        finishtime = employeeshift.ShiftDateEnd;
        //                    }

        //                    if (finishtime != null && starttime != null)
        //                    {
        //                        duration = (finishtime - starttime).Value;
        //                        double? durationminute = (finishtime - starttime).Value.TotalMinutes;
        //                        durationhour = (durationminute / 60);
        //                    }


        //                    // maaş hakedişi dosya ve hareket olarak ekle
        //                    DocumentManager documentManager = new DocumentManager();

        //                    var existssalaryearn = db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == date && x.ActionTypeID == 32 && x.IsActive == true);

        //                    if (existssalaryearn != null)
        //                    {
        //                        SalaryEarn earn = new SalaryEarn();
        //                        earn.ActionTypeID = 32;
        //                        earn.ActionTypeName = "Ücret Hakediş";
        //                        earn.Currency = location.Currency;
        //                        earn.DocumentDate = date;
        //                        earn.EmployeeID = employeeid;
        //                        earn.EnvironmentID = 2;
        //                        earn.LocationID = location.LocationID;
        //                        earn.OurCompanyID = location.OurCompanyID;
        //                        earn.QuantityHour = durationhour.Value;
        //                        earn.ResultID = dayresult != null ? dayresult.ID : (long?)null;
        //                        earn.TimeZone = location.Timezone;
        //                        earn.UID = existssalaryearn.UID.Value;
        //                        earn.Description = dayresult?.Description;

        //                        var res = documentManager.EditSalaryEarn(earn, authentication);
        //                        issuccess = res.IsSuccess;

        //                    }
        //                    else
        //                    {
        //                        SalaryEarn earn = new SalaryEarn();
        //                        earn.ActionTypeID = 32;
        //                        earn.ActionTypeName = "Ücret Hakediş";
        //                        earn.Currency = location.Currency;
        //                        earn.DocumentDate = date;
        //                        earn.EmployeeID = employeeid;
        //                        earn.EnvironmentID = 2;
        //                        earn.LocationID = location.LocationID;
        //                        earn.OurCompanyID = location.OurCompanyID;
        //                        earn.QuantityHour = durationhour.Value;
        //                        earn.ResultID = dayresult != null ? dayresult.ID : (long?)null;
        //                        earn.TimeZone = location.Timezone;
        //                        earn.UID = Guid.NewGuid();

        //                        var res = documentManager.AddSalaryEarn(earn, authentication);
        //                        issuccess = res.IsSuccess;
        //                    }

        //                }

        //            }
        //        }
        //    }

        //    return issuccess;
        //}

        //public static bool CheckEmployeePeriods(EmployeePeriotCheck periodcheck, AuthenticationModel authentication)
        //{
        //    bool issuccess = false;
        //    var date = DateTime.UtcNow.AddHours(authentication.ActionEmployee.OurCompany.TimeZone.Value);
        //    var datedate = date;

        //    if (periodcheck != null)
        //    {
        //        using (ActionTimeEntities db = new ActionTimeEntities())
        //        {
        //            var employee = db.Employee.FirstOrDefault(x => x.EmployeeID == periodcheck.EmployeeID);

        //            //AreaCategoryID
        //            var areacategory = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.AreaCategoryID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && areacategory != null && areacategory.AreaCategoryID != periodcheck.AreaCategoryID && periodcheck.AreaCategoryID != null)
        //            {
        //                if (areacategory.StartDate == datedate)
        //                {
        //                    areacategory.AreaCategoryID = periodcheck.AreaCategoryID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.AreaCategoryID = periodcheck.AreaCategoryID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && areacategory == null && periodcheck.AreaCategoryID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.AreaCategoryID = periodcheck.AreaCategoryID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //DepartmentID
        //            var department = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.DepartmentID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && department != null && department.DepartmentID != periodcheck.DepartmentID && periodcheck.DepartmentID != null)
        //            {
        //                if (department.StartDate == datedate)
        //                {
        //                    department.DepartmentID = periodcheck.DepartmentID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.DepartmentID = periodcheck.DepartmentID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && department == null && periodcheck.DepartmentID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.DepartmentID = periodcheck.DepartmentID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //PositionID
        //            var position = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.PositionID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && position != null && position.PositionID != periodcheck.PositionID && periodcheck.PositionID != null)
        //            {
        //                if (position.StartDate == datedate)
        //                {
        //                    position.PositionID = periodcheck.PositionID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.PositionID = periodcheck.PositionID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && position == null && periodcheck.PositionID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.PositionID = periodcheck.PositionID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //SalaryCategoryID
        //            var salarycategory = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.SalaryCategoryID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && salarycategory != null && salarycategory.SalaryCategoryID != periodcheck.SalaryCategoryID && periodcheck.SalaryCategoryID != null)
        //            {
        //                if (salarycategory.StartDate == datedate)
        //                {
        //                    salarycategory.SalaryCategoryID = periodcheck.SalaryCategoryID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.SalaryCategoryID = periodcheck.SalaryCategoryID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && salarycategory == null && periodcheck.SalaryCategoryID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.SalaryCategoryID = periodcheck.SalaryCategoryID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //SequenceID
        //            var sequence = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.SequenceID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && sequence != null && sequence.SequenceID != periodcheck.SequenceID && periodcheck.SequenceID != null)
        //            {
        //                if (sequence.StartDate == datedate)
        //                {
        //                    sequence.SequenceID = periodcheck.SequenceID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.SequenceID = periodcheck.SequenceID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && sequence == null && periodcheck.SequenceID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.SequenceID = periodcheck.SequenceID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //ShiftTypeID
        //            var shifttype = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.ShiftTypeID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && shifttype != null && shifttype.ShiftTypeID != periodcheck.ShiftTypeID && periodcheck.ShiftTypeID != null)
        //            {
        //                if (shifttype.StartDate == datedate)
        //                {
        //                    shifttype.ShiftTypeID = periodcheck.ShiftTypeID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.ShiftTypeID = periodcheck.ShiftTypeID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && shifttype == null && periodcheck.ShiftTypeID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.ShiftTypeID = periodcheck.ShiftTypeID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //RoleGroupID
        //            var rolegroup = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.RoleGroupID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && rolegroup != null && rolegroup.RoleGroupID != periodcheck.RoleGroupID && periodcheck.RoleGroupID != null)
        //            {
        //                if (rolegroup.StartDate == datedate)
        //                {
        //                    rolegroup.RoleGroupID = periodcheck.RoleGroupID;
        //                    db.SaveChanges();
        //                }
        //                else
        //                {
        //                    EmployeePeriods newperiod = new EmployeePeriods();

        //                    newperiod.RoleGroupID = periodcheck.RoleGroupID;
        //                    newperiod.EmployeeID = periodcheck.EmployeeID;
        //                    newperiod.ContractStartDate = datedate;
        //                    newperiod.OurCompanyID = employee.OurCompanyID;
        //                    newperiod.RecordDate = date;
        //                    newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                    newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                    newperiod.StartDate = datedate;

        //                    db.EmployeePeriods.Add(newperiod);
        //                    db.SaveChanges();
        //                }
        //            }
        //            else if (employee != null && rolegroup == null && periodcheck.RoleGroupID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.RoleGroupID = periodcheck.RoleGroupID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);
        //                db.SaveChanges();
        //            }

        //            //StatusID
        //            var status = db.EmployeePeriods.Where(x => x.EmployeeID == periodcheck.EmployeeID && x.EmployeeStatusID != null).OrderByDescending(x => x.StartDate).ThenByDescending(x => x.RecordDate).FirstOrDefault();
        //            if (employee != null && status != null && status.EmployeeStatusID != periodcheck.StatusID && periodcheck.StatusID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.EmployeeStatusID = periodcheck.StatusID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;
        //                newperiod.ContractStartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);

        //                if (periodcheck.StatusID == 1 && employee.DateStart == null)
        //                {
        //                    employee.DateStart = datedate;
        //                }
        //                else if (periodcheck.StatusID == 2 && employee.DateEnd == null)
        //                {
        //                    employee.DateEnd = datedate;
        //                }

        //                db.SaveChanges();
        //            }
        //            else if (employee != null && status == null && periodcheck.StatusID != null)
        //            {
        //                EmployeePeriods newperiod = new EmployeePeriods();

        //                newperiod.EmployeeStatusID = periodcheck.StatusID;
        //                newperiod.EmployeeID = periodcheck.EmployeeID;
        //                newperiod.ContractStartDate = datedate;
        //                newperiod.OurCompanyID = employee.OurCompanyID;
        //                newperiod.RecordDate = date;
        //                newperiod.RecordedEmployeeID = authentication.ActionEmployee.EmployeeID;
        //                newperiod.RecordIP = OfficeHelper.GetIPAddress();
        //                newperiod.StartDate = datedate;

        //                db.EmployeePeriods.Add(newperiod);

        //                if (periodcheck.StatusID == 1 && employee.DateStart == null)
        //                {
        //                    employee.DateStart = datedate;
        //                }
        //                else if (periodcheck.StatusID == 2 && employee.DateEnd == null)
        //                {
        //                    employee.DateEnd = datedate;
        //                }

        //                db.SaveChanges();
        //            }

        //        }

        //        issuccess = true;
        //    }

        //    return issuccess;
        //}

        //public static void CheckDayResultPriceList(long? id)
        //{
        //    Result<DayResult> result = new Result<DayResult>()
        //    {
        //        IsSuccess = false,
        //        Message = string.Empty
        //    };

        //    using (ActionTimeEntities db = new ActionTimeEntities())
        //    {
        //        ResultControlModel model = new ResultControlModel();

        //        var dayresult = db.DayResult.FirstOrDefault(x => x.ID == id);

        //        if (dayresult != null)
        //        {
        //            List<DayResultCheckPrice> cplist = new List<DayResultCheckPrice>();

        //            var location = db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
        //            var pricelist = db.GetLocationPrice(dayresult.LocationID, dayresult.Date);

        //            foreach (var item in pricelist)
        //            {
        //                var checkprice = db.DayResultCheckPrice.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.Date == dayresult.Date && x.PriceCategoryID == item.PriceCategoryID && x.PriceID == item.ID && x.ResultID == dayresult.ID && x.Unit == item.Unit);
        //                if (checkprice != null)
        //                {
        //                    DayResultCheckPrice self = new DayResultCheckPrice()
        //                    {
        //                        Currency = checkprice.Currency,
        //                        RecordEmployeeID = checkprice.RecordEmployeeID,
        //                        RecordDate = checkprice.RecordDate,
        //                        Quantity = checkprice.Quantity,
        //                        Total = checkprice.Total,
        //                        Date = checkprice.Date,
        //                        ID = checkprice.ID,
        //                        LocationID = checkprice.LocationID,
        //                        Price = checkprice.Price,
        //                        PriceCategoryID = checkprice.PriceCategoryID,
        //                        PriceID = checkprice.PriceID,
        //                        RecordIP = checkprice.RecordIP,
        //                        ResultID = checkprice.ResultID,
        //                        Sign = checkprice.Sign,
        //                        Unit = checkprice.Unit,
        //                        UpdateDate = checkprice.UpdateDate,
        //                        UpdateEmployeeID = checkprice.UpdateEmployeeID,
        //                        UpdateIP = checkprice.UpdateIP
        //                    };

        //                    checkprice.Currency = item.Currency;
        //                    checkprice.Price = item.Price;
        //                    checkprice.Total = (checkprice.Total * checkprice.Price);
        //                    checkprice.UpdateDate = location.LocalDateTime;
        //                    checkprice.UpdateEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
        //                    checkprice.UpdateIP = GetIPAddress();

        //                    db.SaveChanges();

        //                    var isequal = PublicInstancePropertiesEqual<DayResultCheckPrice>(self, checkprice, getIgnorelist());
        //                    AddApplicationLog("Office", "DayResultCheckPrice", "Update", checkprice.ID.ToString(), "Result", "CheckDayResultPriceList", isequal, true, $"{checkprice.Unit} - {checkprice.Price} {item.Currency}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), $"{model.Authentication.ActionEmployee.EmployeeID} {model.Authentication.ActionEmployee.FullName}", GetIPAddress(), string.Empty, null);

        //                }
        //                else
        //                {
        //                    DayResultCheckPrice cp = new DayResultCheckPrice();

        //                    cp.Date = dayresult.Date;
        //                    cp.LocationID = dayresult.LocationID;
        //                    cp.Price = item.Price;
        //                    cp.PriceCategoryID = item.PriceCategoryID;
        //                    cp.PriceID = item.ID;
        //                    cp.Quantity = 0;
        //                    cp.RecordDate = location.LocalDateTime;
        //                    cp.RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID;
        //                    cp.RecordIP = GetIPAddress();
        //                    cp.ResultID = dayresult.ID;
        //                    cp.Total = (cp.Quantity * cp.Price);
        //                    cp.Unit = item.Unit;
        //                    cp.Currency = item.Currency;
        //                    cp.Sign = item.Sign;

        //                    cplist.Add(cp);


        //                    AddApplicationLog("Office", "DayResultCheckPrice", "Insert", cp.ID.ToString(), "Result", "AddItemsToResultEnvelope", null, true, $"{cp.Unit} - {cp.Price} {item.Currency}", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), $"{model.Authentication.ActionEmployee.EmployeeID} {model.Authentication.ActionEmployee.FullName}", OfficeHelper.GetIPAddress(), string.Empty, cp);

        //                }
        //            }

        //            db.DayResultCheckPrice.AddRange(cplist);
        //            db.SaveChanges();

        //        }
        //    }
        //}


    }
}
