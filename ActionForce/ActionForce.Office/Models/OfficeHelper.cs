using ActionForce.Entity;
using ActionForce.Integration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ActionForce.Office
{
    public class OfficeHelper
    {
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

        public static IEnumerable<FromAccountModel> GetFromList(int ourCompanyID)
        {

            List<FromAccountModel> fromList = new List<FromAccountModel>();


            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                fromList = db.GetFromList(ourCompanyID).Select(x => new FromAccountModel()
                {
                    ID = x.ID,
                    Code = x.Code,
                    Name = x.Name,
                    Prefix = x.Prefix
                }).ToList();
            }


            return fromList;
        }



        public static IEnumerable<Currency> GetCurrency()
        {
            List<Currency> currList = new List<Currency>();
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                currList = db.Currency.ToList();
            }
            return currList;
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
                    Cash newcash = new Cash();

                    newcash.BlockedAmount = 100;
                    newcash.CashName = $"Lokasyon Kasası";
                    newcash.Currency = currency;
                    newcash.IsActive = true;
                    newcash.LocationID = locationID;
                    newcash.SortBy = "01";

                    db.Cash.Add(newcash);
                    db.SaveChanges();

                    return newcash;
                }
            }

        }

        public static int GetTimeZone(int locationID)
        {
            int timezone = 3;

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                timezone = db.Location.FirstOrDefault(x => x.LocationID == locationID)?.Timezone ?? 3;
                return timezone;
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

        public static void AddCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.AddCashAction(CashID, LocationID, EmployeeID, CashActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
            }
        }

        public static void AddBankAction(int? LocationID, int? EmployeeID, int? BankAccountID, int? PosID, int? BankActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.AddBankAction(LocationID, EmployeeID, BankAccountID, PosID, BankActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
            }
        }

        public static void AddEmployeeAction(int? EmployeeID, int? LocationID, int? ActionTypeID, string ProcessName, long? ProcessID, DateTime? ProcessDate, string ProcessDetail, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? SalaryTypeID, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID, string DocumentNumber, int SalaryCategoryID)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.AddEmployeeAction(EmployeeID, LocationID, ActionTypeID, ProcessName, ProcessID, ProcessDate, ProcessDetail, Direction, Collection, Payment, Currency, Latitude, Longitude, SalaryTypeID, RecordEmployeeID, RecordDate, ProcessUID, DocumentNumber, SalaryCategoryID);
            }
        }

        public static void AddCustomerAction(int? CustomerID, int? LocationID, int? EmployeeID, int? ActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, Guid ProcessUID)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.AddCustomerAction(CustomerID, LocationID, EmployeeID, ActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, ProcessUID);
            }
        }

        public static void UpdateCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate, int? UpdateEmployeeID, DateTime? UpdateDate)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.UpdateCashAction(CashID, LocationID, EmployeeID, CashActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate, UpdateEmployeeID, UpdateDate);
            }
        }

        public static Result<DayResult> AddItemsToResultEnvelope(long? id)
        {

            // ID Module  Category ItemName
            //1   Cash NULL    Kasa Tahsilatı
            //2   Cash NULL    Kasa Satışı
            //3   Cash NULL    Döviz Satışı
            //4   Cash NULL    Kasa Ödemesi
            //5   Cash NULL    Kasa Satış İadesi
            //6   Cash NULL    Kasadan Masraf Ödemesi
            //7   Cash NULL    Havale / EFT
            //8   Cash NULL    Maaş Hakedişi
            //9   Cash NULL    Maaş / Avans Ödemesi
            //10  Bank NULL    Pos(Kredi Kartı)  Satışı
            //11  Bank NULL    Pos(Kredi Kartı)  Satış İptali
            //12  Bank NULL    Pos(Kredi Kartı)  Satış İadesi
            //13  CashRecorder NULL    Yazarkasa Fişi
            //14  File NULL    Dosyalar

            Result<DayResult> result = new Result<DayResult>()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                ResultControlModel model = new ResultControlModel();

                var dayresult = db.DayResult.FirstOrDefault(x => x.ID == id);

                if (dayresult != null)
                {

                    List<DayResultItemList> itemlist = new List<DayResultItemList>();

                    var items = db.DayResultItems.ToList();
                    var location = db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);

                    if (location.OurCompanyID == 2)  // ülke türkiye ise
                    {
                        result.IsSuccess = true;

                        // 01 Kasa Tahsilatı ekle
                        var exchange = GetExchange(dayresult.Date);

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "USD",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.USDA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //eur
                        itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "EUR",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.EURA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 02 Kasa Satışı ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "USD",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.USDA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //eur
                        itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "EUR",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.EURA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 03 Kasa Döviz Satışı ekle

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "USD",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.USDA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "EUR",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.EURA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 04 Kasa Ödemesi Ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "USD",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.USDA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //eur
                        itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "EUR",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.EURA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 05 Kasa Satış İadesi Ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "USD",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.USDA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //eur
                        itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "EUR",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = exchange.EURA,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 06 Kasadan Masraf Ödemesi Ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 6).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 07 Kasadan Havale EFT çıkma Ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 7).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 1,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());




                        // 08 Maaş Hakedişi işle Maaş Avans Ödemesi Ekle

                        var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

                        if (locschedule != null)
                        {
                            var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
                            List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();

                            var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
                            var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

                            foreach (var emp in empids)
                            {
                                var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID, model.Authentication);

                                //var employeeschedule = empschedules.FirstOrDefault(x => x.EmployeeID == emp);
                                //var employeeshift = empshifts.FirstOrDefault(x => x.EmployeeID == emp);
                                //var hourprice = empunits.Where(x => x.EmployeeID == emp && x.Hourly > 0 && x.DateStart <= dayresult.Date).OrderByDescending(x => x.DateStart).FirstOrDefault();

                                //double? durationhour = 0;
                                //double? unithour = hourprice?.Hourly ?? 0;
                                //TimeSpan? duration = null;

                                //if (employeeschedule != null && employeeshift != null)
                                //{
                                //    DateTime? starttime = employeeschedule.ShiftDateStart;
                                //    if (employeeshift.ShiftDateStart > starttime)
                                //    {
                                //        starttime = employeeshift.ShiftDateStart;
                                //    }

                                //    DateTime? finishtime = employeeschedule.ShiftdateEnd;
                                //    if (employeeshift.ShiftDateEnd < finishtime)
                                //    {
                                //        finishtime = employeeshift.ShiftDateEnd;
                                //    }

                                //    if (finishtime != null && starttime != null)
                                //    {
                                //        duration = (finishtime - starttime).Value;
                                //        double? durationminute = (finishtime - starttime).Value.TotalMinutes;
                                //        durationhour = (durationminute / 60);
                                //    }


                                //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = (durationhour * unithour),
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = (durationhour * unithour),
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        SystemHourQuantity = durationhour,
                                //        UnitHourPrice = unithour,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress(),
                                //        Duration = duration,
                                //        SystemDuration = duration,
                                //        HourQuantity = durationhour

                                //    }).FirstOrDefault());

                                //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = 0,
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = 0,
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress()

                                //    }).FirstOrDefault());

                                //}
                                //else
                                //{
                                //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = 0,
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = 0,
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        SystemHourQuantity = 0,
                                //        UnitHourPrice = unithour,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress()

                                //    }).FirstOrDefault());

                                //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = 0,
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = 0,
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress()

                                //    }).FirstOrDefault());
                                //}
                            }


                        }
                        else
                        {
                            result.Message = "Lokasyon takvimi tanımlanmamış";
                        }

                        // 10 POS Kredi Kartı Satışı Ekle

                        itemlist.Add(items.Where(x => x.ID == 10).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 8,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 11 POS Kredi Kartı Satışı İptali Ekle

                        itemlist.Add(items.Where(x => x.ID == 11).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 8,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 12 POS Kredi Kartı Satışı İadesi Ekle

                        itemlist.Add(items.Where(x => x.ID == 12).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 8,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        // 13 Yazarkasa satışı fişi Ekle

                        itemlist.Add(items.Where(x => x.ID == 13).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress(),
                            SlipNumber = string.Empty,
                            SlipDate = dayresult.Date.AddHours(22),
                            SlipTotalAmount = 0

                        }).FirstOrDefault());

                    }
                    else if (location.OurCompanyID == 1)  // ülke amerika ise
                    {
                        result.IsSuccess = true;

                        // 01 Kasa Tahsilatı ekle
                        var exchange = GetExchange(dayresult.Date);

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 1).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 02 Kasa Satışı ekle

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 2).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 03 Kasa Döviz Satışı ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "TRL",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = (1 / exchange.USDA),
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        //eur
                        itemlist.Add(items.Where(x => x.ID == 3).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = "EUR",
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = (1 / exchange.EURA),
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 04 Kasa Ödemesi Ekle

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 4).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 05 Kasa Satış İadesi Ekle

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 5).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 06 Kasadan Masraf Ödemesi Ekle

                        //trl
                        itemlist.Add(items.Where(x => x.ID == 6).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 07 Kasadan Havale EFT çıkma Ekle

                        //usd
                        itemlist.Add(items.Where(x => x.ID == 7).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 4,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());




                        // 08 Maaş Hakedişi işle Maaş Avans Ödemesi Ekle

                        var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

                        if (locschedule != null)
                        {
                            var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
                            List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();


                            var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
                            var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

                            foreach (var emp in empids)
                            {

                                var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID, model.Authentication);

                                //var employeeschedule = empschedules.FirstOrDefault(x => x.EmployeeID == emp);
                                //var employeeshift = empshifts.FirstOrDefault(x => x.EmployeeID == emp);
                                //var hourprice = empunits.Where(x => x.EmployeeID == emp && x.Hourly > 0 && x.DateStart <= dayresult.Date).OrderByDescending(x => x.DateStart).FirstOrDefault();

                                //double? durationhour = 0;
                                //double? unithour = hourprice?.Hourly ?? 0;
                                //TimeSpan? duration = null;

                                //if (employeeschedule != null && employeeshift != null)
                                //{
                                //    DateTime? starttime = employeeschedule.ShiftDateStart;
                                //    if (employeeshift.ShiftDateStart > starttime)
                                //    {
                                //        starttime = employeeshift.ShiftDateStart;
                                //    }

                                //    DateTime? finishtime = employeeschedule.ShiftdateEnd;
                                //    if (employeeshift.ShiftDateEnd < finishtime)
                                //    {
                                //        finishtime = employeeshift.ShiftDateEnd;
                                //    }

                                //    if (finishtime != null && starttime != null)
                                //    {
                                //        duration = (finishtime - starttime).Value;
                                //        double? durationminute = (finishtime - starttime).Value.TotalMinutes;
                                //        durationhour = (durationminute / 60);
                                //    }

                                //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = (durationhour * hourprice.Hourly),
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = (durationhour * hourprice.Hourly),
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        SystemHourQuantity = durationhour,
                                //        UnitHourPrice = hourprice.Hourly,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress(),
                                //        Duration = duration,
                                //        SystemDuration = duration,
                                //        HourQuantity = durationhour

                                //    }).FirstOrDefault());

                                //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = 0,
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = 0,
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress()

                                //    }).FirstOrDefault());

                                //}
                                //else
                                //{
                                //    itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = 0,
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = 0,
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        SystemHourQuantity = 0,
                                //        UnitHourPrice = hourprice.Hourly,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress()

                                //    }).FirstOrDefault());

                                //    itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                //    {
                                //        Amount = 0,
                                //        Category = x.Category,
                                //        Currency = location.Currency,
                                //        ResultID = id,
                                //        ResultItemID = x.ID,
                                //        Quantity = 0,
                                //        SystemQuantity = 0,
                                //        Exchange = 1,
                                //        SystemAmount = 0,
                                //        LocationID = location.LocationID,
                                //        Date = dayresult.Date,
                                //        EmployeeID = emp,
                                //        RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                //        RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                                //        RecordIP = GetIPAddress()

                                //    }).FirstOrDefault());
                                //}
                            }


                        }
                        else
                        {
                            result.Message = "Lokasyon takvimi tanımlanmamış";
                        }

                        // 10 POS Kredi Kartı Satışı Ekle

                        itemlist.Add(items.Where(x => x.ID == 10).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 7,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 11 POS Kredi Kartı Satışı İptali Ekle

                        itemlist.Add(items.Where(x => x.ID == 11).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 7,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());


                        // 12 POS Kredi Kartı Satışı İadesi Ekle

                        itemlist.Add(items.Where(x => x.ID == 12).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            BankAccountID = 7,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress()

                        }).FirstOrDefault());

                        // 13 Yazarkasa satışı fişi Ekle

                        itemlist.Add(items.Where(x => x.ID == 13).Select(x => new DayResultItemList()
                        {
                            Amount = 0,
                            Category = x.Category,
                            Currency = location.Currency,
                            ResultID = id,
                            ResultItemID = x.ID,
                            Quantity = 0,
                            SystemQuantity = 0,
                            Exchange = 1,
                            SystemAmount = 0,
                            LocationID = location.LocationID,
                            Date = dayresult.Date,
                            RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                            RecordEmployeeID = model.Authentication.ActionEmployee.EmployeeID,
                            RecordIP = GetIPAddress(),
                            SlipNumber = string.Empty,
                            SlipDate = dayresult.Date.AddHours(22),
                            SlipTotalAmount = 0

                        }).FirstOrDefault());

                    }

                    db.DayResultItemList.AddRange(itemlist);
                    db.SaveChanges();
                }

            }




            return result;

        }

        public static string BankReferenceCode(int OurCompanyID)
        {
            string rndNumber = string.Empty;

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                List<string> numberlist = db.DocumentBankTransfer.Select(x => x.ReferenceCode).ToList();

                rndNumber = OurCompanyID.ToString() + DateTime.Now.ToString("yy");

                Random rnd = new Random();

                for (int i = 1; i < 6; i++)
                {
                    rndNumber += rnd.Next(0, 9).ToString();
                }

                if (!numberlist.Contains(rndNumber))
                {
                    return rndNumber;
                }
                else
                {
                    return BankReferenceCode(OurCompanyID);
                }
            }
        }

        public static bool CalculateSalaryEarn(long? resultid, int employeeid, DateTime? date, int? locationid, AuthenticationModel authentication)
        {
            bool issuccess = false;

            if (employeeid > 0 && (resultid > 0 || date != null || locationid > 0))
            {

                using (ActionTimeEntities db = new ActionTimeEntities())
                {
                    var dayresult = db.VDayResult.FirstOrDefault(x => x.ID == resultid.Value);

                    if (dayresult == null)
                    {
                        date = date.Value.Date;
                        dayresult = db.VDayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
                    }


                    if (dayresult != null)
                    {
                        List<DayResultItemList> itemlist = new List<DayResultItemList>();

                        var items = db.DayResultItems.ToList();

                        var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date);
                        var location = db.Location.FirstOrDefault(x => x.LocationID == dayresult.LocationID);
                        var locationstats = db.LocationStats.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.StatsID == 2 && x.OptionID == 3);


                        if (locschedule != null)
                        {
                            var employeeschedule = db.Schedule.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date && x.EmployeeID == employeeid);

                            var employeeshift = db.EmployeeShift.FirstOrDefault(x => x.LocationID == dayresult.LocationID && x.ShiftDate == dayresult.Date && x.EmployeeID == employeeid);
                            var empunits = db.EmployeeSalary.Where(x => x.EmployeeID == employeeid && x.DateStart <= dayresult.Date).ToList();
                            var hourprice = empunits.Where(x => x.Hourly > 0 && x.DateStart <= dayresult.Date).OrderByDescending(x => x.DateStart).FirstOrDefault();

                            double? durationhour = 0;
                            double? unithour = hourprice?.Hourly ?? 0;


                            if (location.OurCompanyID == 1 && locationstats != null)
                            {
                                unithour = unithour + 1;
                            }


                            TimeSpan? duration = null;

                            if (employeeschedule != null && employeeshift != null)
                            {
                                DateTime? starttime = employeeschedule.ShiftDateStart;
                                if (employeeshift.ShiftDateStart > starttime)
                                {
                                    starttime = employeeshift.ShiftDateStart;
                                }

                                DateTime? finishtime = employeeschedule.ShiftdateEnd;
                                if (employeeshift.ShiftDateEnd < finishtime)
                                {
                                    finishtime = employeeshift.ShiftDateEnd;
                                }

                                if (finishtime != null && starttime != null)
                                {
                                    duration = (finishtime - starttime).Value;
                                    double? durationminute = (finishtime - starttime).Value.TotalMinutes;
                                    durationhour = (durationminute / 60);
                                }

                                // varmı yokmu
                                var existsitem = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 8);
                                if (existsitem != null)
                                {
                                    db.DayResultItemList.Remove(existsitem);
                                    db.SaveChanges();
                                }

                                itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                {
                                    Amount = (durationhour * unithour),
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = (durationhour * unithour),
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    SystemHourQuantity = durationhour,
                                    UnitHourPrice = unithour,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
                                    RecordIP = GetIPAddress(),
                                    Duration = duration,
                                    SystemDuration = duration,
                                    HourQuantity = durationhour

                                }).FirstOrDefault());

                                var existsitem9 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 9);
                                if (existsitem9 != null)
                                {
                                    db.DayResultItemList.Remove(existsitem9);
                                    db.SaveChanges();
                                }

                                itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
                                    RecordIP = GetIPAddress()

                                }).FirstOrDefault());

                                // maaş hakedişi dosya ve hareket olarak ekle

                                var existssalaryearn = db.DocumentSalaryEarn.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date);

                                if (existssalaryearn != null)
                                {

                                    DocumentSalaryEarn self = new DocumentSalaryEarn()
                                    {
                                        ActionTypeID = existssalaryearn.ActionTypeID,
                                        ActionTypeName = existssalaryearn.ActionTypeName,
                                        Currency = existssalaryearn.Currency,
                                        Date = existssalaryearn.Date,
                                        Description = existssalaryearn.Description,
                                        DocumentNumber = existssalaryearn.DocumentNumber,
                                        EmployeeID = existssalaryearn.EmployeeID,
                                        EnvironmentID = existssalaryearn.EnvironmentID,
                                        ID = existssalaryearn.ID,
                                        IsActive = existssalaryearn.IsActive,
                                        LocationID = existssalaryearn.LocationID,
                                        OurCompanyID = existssalaryearn.OurCompanyID,
                                        QuantityHour = existssalaryearn.QuantityHour,
                                        RecordDate = existssalaryearn.RecordDate,
                                        RecordEmployeeID = existssalaryearn.RecordEmployeeID,
                                        RecordIP = existssalaryearn.RecordIP,
                                        ReferenceID = existssalaryearn.ReferenceID,
                                        ResultID = existssalaryearn.ResultID,
                                        SystemQuantityHour = existssalaryearn.SystemQuantityHour,
                                        SystemTotalAmount = existssalaryearn.SystemTotalAmount,
                                        SystemUnitPrice = existssalaryearn.SystemUnitPrice,
                                        TotalAmount = existssalaryearn.TotalAmount,
                                        UID = existssalaryearn.UID,
                                        UnitPrice = existssalaryearn.UnitPrice,
                                        UpdateDate = existssalaryearn.UpdateDate,
                                        UpdateEmployee = existssalaryearn.UpdateEmployee,
                                        UpdateIP = existssalaryearn.UpdateIP
                                    };


                                    existssalaryearn.QuantityHour = durationhour.Value;
                                    existssalaryearn.SystemQuantityHour = durationhour.Value;
                                    existssalaryearn.TotalAmount = (durationhour.Value * unithour.Value);
                                    existssalaryearn.SystemTotalAmount = (durationhour.Value * unithour.Value);
                                    existssalaryearn.UnitPrice = unithour;
                                    existssalaryearn.SystemUnitPrice = unithour;
                                    existssalaryearn.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                                    existssalaryearn.UpdateEmployee = authentication.ActionEmployee.EmployeeID;
                                    existssalaryearn.UpdateIP = OfficeHelper.GetIPAddress();

                                    //log at
                                    db.SaveChanges();

                                    var empaction = db.EmployeeCashActions.FirstOrDefault(x => x.EmployeeID == employeeid && x.ActionTypeID == 32 && x.ProcessID == existssalaryearn.ID && x.ProcessDate == dayresult.Date && x.ProcessUID == existssalaryearn.UID);

                                    if (empaction != null)
                                    {

                                        empaction.UpdateDate = DateTime.UtcNow.AddHours(location.Timezone.Value);
                                        empaction.UpdateEmployeeID = authentication.ActionEmployee.EmployeeID;
                                        empaction.Collection = (durationhour.Value * unithour.Value);
                                        empaction.Payment = 0;

                                        db.SaveChanges();

                                    }
                                    else
                                    {
                                        OfficeHelper.AddEmployeeAction(existssalaryearn.EmployeeID, existssalaryearn.LocationID, existssalaryearn.ActionTypeID, existssalaryearn.ActionTypeName, existssalaryearn.ID, existssalaryearn.Date, existssalaryearn.Description, 1, existssalaryearn.TotalAmount, 0, existssalaryearn.Currency, null, null, null, existssalaryearn.RecordEmployeeID, existssalaryearn.RecordDate, existssalaryearn.UID.Value, existssalaryearn.DocumentNumber, 3);
                                    }


                                    var isequal = OfficeHelper.PublicInstancePropertiesEqual<DocumentSalaryEarn>(self, existssalaryearn, OfficeHelper.getIgnorelist());
                                    OfficeHelper.AddApplicationLog("Office", "DocumentSalaryEarn", "Update", existssalaryearn.ID.ToString(), "Result", "SalaryEarn", isequal, true, $"Hakediş güncellendi", string.Empty, DateTime.UtcNow.AddHours(location.Timezone.Value), authentication.ActionEmployee.FullName, OfficeHelper.GetIPAddress(), string.Empty, null);



                                }
                                else
                                {
                                    DocumentManager documentManager = new DocumentManager();

                                    SalaryEarn earn = new SalaryEarn();

                                    earn.ActionTypeID = 32;
                                    earn.ActionTypeName = "Ücret Hakediş";
                                    earn.Currency = location.Currency;
                                    earn.DocumentDate = dayresult.Date;
                                    earn.EmployeeID = employeeid;
                                    earn.EnvironmentID = 2;
                                    earn.LocationID = location.LocationID;
                                    earn.OurCompanyID = location.OurCompanyID;
                                    earn.QuantityHour = durationhour.Value;
                                    earn.ResultID = dayresult.ID;
                                    earn.SystemTotalAmount = (durationhour * unithour);
                                    earn.SystemQuantityHour = durationhour;
                                    earn.SystemUnitPrice = unithour;
                                    earn.TimeZone = location.Timezone;
                                    earn.TotalAmount = (durationhour.Value * unithour.Value);
                                    earn.UID = Guid.NewGuid();
                                    earn.UnitPrice = unithour;


                                    var res = documentManager.AddSalaryEarn(earn, authentication);  // log zaten var.
                                }

                            }
                            else
                            {

                                var existsitem8 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 8);
                                if (existsitem8 != null)
                                {
                                    db.DayResultItemList.Remove(existsitem8);
                                    db.SaveChanges();
                                }

                                var existsitem9 = db.DayResultItemList.FirstOrDefault(x => x.LocationID == location.LocationID && x.EmployeeID == employeeid && x.Date == dayresult.Date && x.ResultItemID == 9);
                                if (existsitem9 != null)
                                {
                                    db.DayResultItemList.Remove(existsitem9);
                                    db.SaveChanges();
                                }

                                itemlist.Add(items.Where(x => x.ID == 8).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    SystemHourQuantity = 0,
                                    UnitHourPrice = unithour,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
                                    RecordIP = GetIPAddress()

                                }).FirstOrDefault());

                                itemlist.Add(items.Where(x => x.ID == 9).Select(x => new DayResultItemList()
                                {
                                    Amount = 0,
                                    Category = x.Category,
                                    Currency = location.Currency,
                                    ResultID = dayresult.ID,
                                    ResultItemID = x.ID,
                                    Quantity = 0,
                                    SystemQuantity = 0,
                                    Exchange = 1,
                                    SystemAmount = 0,
                                    LocationID = location.LocationID,
                                    Date = dayresult.Date,
                                    EmployeeID = employeeid,
                                    RecordDate = DateTime.UtcNow.AddHours(location.Timezone.Value),
                                    RecordEmployeeID = authentication.ActionEmployee.EmployeeID,
                                    RecordIP = GetIPAddress()

                                }).FirstOrDefault());


                            }
                        }

                        db.DayResultItemList.AddRange(itemlist);
                        db.SaveChanges();
                    }
                }
            }

            return issuccess;
        }

        public static bool CheckSalaryEarn(long? resultid, DateTime? date, int? locationid, AuthenticationModel authentication)
        {
            bool issuccess = false;

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                var location = db.Location.FirstOrDefault(x => x.LocationID == locationid);
                var dayresult = db.DayResult.FirstOrDefault(x => x.LocationID == locationid && x.Date == date);
                var locschedule = db.LocationSchedule.FirstOrDefault(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date);

                if (locschedule != null)
                {
                    var empschedules = db.Schedule.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date).ToList();
                    List<int> empids = empschedules.Select(x => x.EmployeeID.Value).ToList();


                    var empshifts = db.EmployeeShift.Where(x => x.LocationID == location.LocationID && x.ShiftDate == dayresult.Date && empids.Contains(x.EmployeeID.Value)).ToList();
                    var empunits = db.EmployeeSalary.Where(x => empids.Contains(x.EmployeeID) && x.DateStart <= dayresult.Date).ToList();

                    foreach (var emp in empids)
                    {
                        var calculate = CalculateSalaryEarn(dayresult.ID, emp, dayresult.Date, dayresult.LocationID, authentication);
                    }

                    issuccess = true;
                }
            }

            return issuccess;
        }

    }
}