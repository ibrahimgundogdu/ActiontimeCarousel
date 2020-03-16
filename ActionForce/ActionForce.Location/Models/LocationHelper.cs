using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ActionForce.Location
{
    public class LocationHelper
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

        public static TicketInfo GetScannedTicketInfo(string scannedNumber, VLocation location)  //   0083|UFE2L154FSFBHR5K2ZIEG|2019-10-01|S191001637055373703567587
        {
            TicketInfo model = new TicketInfo();

            Result result = new Result()
            {
                IsSuccess = false,
                Message = string.Empty
            };

            if (!string.IsNullOrEmpty(scannedNumber))
            {
                string[] scannedParts = scannedNumber.Split('|');

                if (scannedParts.Count() == 4)
                {
                    int locationID = Convert.ToInt32(scannedParts[0]);
                    string ticketUID = scannedParts[1];
                    DateTime orderDate = Convert.ToDateTime(scannedParts[2]);
                    string orderNumber = scannedParts[3];

                    using (ActionTimeEntities db = new ActionTimeEntities())
                    {
                        var isexists = db.VTicketSaleRowCheck.FirstOrDefault(x => x.LocationID == locationID && x.UID.ToString() == ticketUID && x.OrderNumber == orderNumber && x.SaleDate == orderDate);
                       
                        if (isexists != null)
                        {
                            var isbasket = db.TicketBasket.FirstOrDefault(x => x.TicketNumber == isexists.TicketNumber);

                            model.Info = isexists;
                           
                            if (isexists.IsBlocked == false && isexists.IsActive == true && isexists.Status == 2 && isexists.StatusID == 2 && isexists.Currency == location.Currency && isexists.TicketTypeID == location.TicketTypeID && isbasket == null)
                            {
                                result.IsSuccess = true;
                                result.Message = "Bilet Kullanılabilir";
                            }
                            else if (isexists.IsBlocked)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bilet Bloke Edilmiş";
                            }
                            else if (isexists.Status != 2 && isexists.StatusID != 2)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bilet aşaması uygun değil : " + isexists.StatusName;
                            }
                            else if (isexists.Currency != location.Currency)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bilet para birimi lokasyonun para biriminden farklı";
                            }
                            else if (isexists.TicketTypeID != location.TicketTypeID)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bilet türü lokasyonun türünden farklı";
                            }
                            else if (isexists.IsActive == false || isexists.IsActive == null)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bilet siparişi pasif edilmiş";
                            }
                            else if (isbasket != null)
                            {
                                result.IsSuccess = false;
                                result.Message = "Bilet sepete zaten eklenmiş";
                            }
                        }
                        else
                        {
                            result.IsSuccess = false;
                            result.Message = "Bilet bilgisine ulaşılamadı";
                        }
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "Okunan seri doğru formatta değil";
                }
            }
            else
            {
                result.Message = "Okunan alan boş olamaz ";
            }

            model.Result = result;

            return model;
        }

    }
}