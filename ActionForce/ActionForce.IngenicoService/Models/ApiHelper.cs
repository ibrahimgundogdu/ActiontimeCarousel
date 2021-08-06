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

        public static string PasswordMD5_Pan(string password)
        {

            password = makeMD5(password);

            if (password.Length > 10)
            {
                password = $"{password.Substring(0, 5)}*****{password.Substring(password.Length - 5, 5)}";
            }
            else if (password.Length <= 10 && password.Length > 3)
            {
                password = $"{password.Substring(0, 1)}***{password.Substring(password.Length - 1, 1)}";
            }

            return password;
        }

        public static void AddPosServiceLog(int LocationID, string PosSerialNumber, string AdisyonNo, string Username, string Password, string MethodName, string RequestData, string ResultCode, string ResultMessage)
        {
            try
            {
                using (ActionTimeEntities Db = new ActionTimeEntities())
                {
                    Db.AddPosServiceLog(LocationID, PosSerialNumber, AdisyonNo, Username, Password, ApiHelper.GetIPAddress(), MethodName, DateTime.UtcNow, RequestData, ResultCode, ResultMessage);
                }
            }
            catch (Exception ex)
            {
            }


        }
    }
}