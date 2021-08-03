using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ActionForce.PosService
{
    public class ApiHelper
    {
        public static bool CheckUserAuthentication(Header Header_Info)
        {
            bool issucces = false;
            ActionTimeEntities Db = new ActionTimeEntities();

            string password = makeMD5(Header_Info.Password);

            var isUser = Db.Employee.Any(x => x.Username == Header_Info.UserName && x.Password == password && x.RoleGroupID == 13 && x.RoleID == 14 && x.IsActive == true);

            return isUser;
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
    }
}