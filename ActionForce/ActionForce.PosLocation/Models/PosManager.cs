using ActionForce.Entity;
using System;
using System.Collections.Generic;
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
                    PhotoFile = x.PhotoFile
                }).ToList();

            }
            return EmployeeList;
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



    }
}