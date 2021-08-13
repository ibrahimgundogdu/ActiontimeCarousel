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
    }
}