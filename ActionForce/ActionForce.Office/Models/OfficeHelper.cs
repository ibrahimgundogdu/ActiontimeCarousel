using ActionForce.Entity;
using ActionForce.Integration;
using System;
using System.Collections.Generic;
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

        public static void AddApplicationLog(string environment, string module, string processType, string processId, string controller, string action, IEnumerable<AuditTrail> differents, bool isSuccess, string resultMessage, string errorMessage, DateTime recordDate, string recordEmployee, string recordIP, string recordDevice)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                if (differents != null)
                {
                    if (differents.Count() > 0)
                    {
                        foreach (var item in differents)
                        {
                            db.AddApplicationLog(environment, module, processType, processId, controller, action, item.TableName, item.FieldName, item.OldValue, item.NewValue, isSuccess, resultMessage, errorMessage, recordDate, recordEmployee, recordIP, recordDevice);
                        }
                    }
                    else
                    {
                        db.AddApplicationLog(environment, module, processType, processId, controller, action, string.Empty, string.Empty, string.Empty, string.Empty, isSuccess, resultMessage, errorMessage, recordDate, recordEmployee, recordIP, recordDevice);

                    }

                }
                else
                {
                    db.AddApplicationLog(environment, module, processType, processId, controller, action, string.Empty, string.Empty, string.Empty, string.Empty, isSuccess, resultMessage, errorMessage, recordDate, recordEmployee, recordIP, recordDevice);

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


            if (DateTime.UtcNow.Hour >= 12 && DateTime.UtcNow.Minute > 0)
            {
                date = date.AddDays(1).Date;
            }
            else
            {
                date = date.Date;
            }

            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                var exchange = db.Exchange.FirstOrDefault(x => x.Date == date);

                if (exchange != null)
                {
                    return exchange;
                }
                else
                {
                    var datekey = db.DateList.FirstOrDefault(x => x.DateKey == date);

                    if (datekey.DayName == "Saturday" || datekey.DayName =="Sunday")
                    {
                        date = db.DateList.FirstOrDefault(x => x.Year == datekey.Year && x.WeekNumber == datekey.WeekNumber && x.DayName == "Friday").DateKey;
                    }

                    TCMBClient tcmbClient = new TCMBClient();

                    string dateparam = date.ToString("dd-MM-yyyy");

                    var newExchange = tcmbClient.GetExchangeToday(dateparam);

                    if (newExchange != null)
                    {
                        Exchange newexchange = new Exchange();

                        var item = newExchange.items.FirstOrDefault();
                        newexchange.Date = date;
                        newexchange.EURA = Convert.ToDouble(item.TP_DK_EUR_A.Replace(".", ","));
                        newexchange.EURS = Convert.ToDouble(item.TP_DK_EUR_S.Replace(".", ","));
                        newexchange.USDA = Convert.ToDouble(item.TP_DK_USD_A.Replace(".", ","));
                        newexchange.USDS = Convert.ToDouble(item.TP_DK_USD_S.Replace(".", ","));

                        db.Exchange.Add(newexchange);
                        db.SaveChanges();

                        return newexchange;
                    }

                }
            }

            return null;

        }

        public static void AddCashAction(int? CashID, int? LocationID, int? EmployeeID, int? CashActionTypeID, DateTime? ActionDate, string ProcessName, long? ProcessID, DateTime? ProcessDate, string DocumentNumber, string Description, short? Direction, double? Collection, double? Payment, string Currency, double? Latitude, double? Longitude, int? RecordEmployeeID, DateTime? RecordDate)
        {
            using (ActionTimeEntities db = new ActionTimeEntities())
            {
                db.AddCashAction(CashID, LocationID, EmployeeID, CashActionTypeID, ActionDate, ProcessName, ProcessID, ProcessDate, DocumentNumber, Description, Direction, Collection, Payment, Currency, Latitude, Longitude, RecordEmployeeID, RecordDate);
            }
        }
    }
}