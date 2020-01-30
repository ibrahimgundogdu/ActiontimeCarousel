using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class ApiHelper
    {
        public static bool UserAuthentication(string Token)
        {
            bool issucces = false;
            ActionTimeEntities Db = new ActionTimeEntities();
            string message = "Login Failed";

            //var user = Db.Users.FirstOrDefault(x => x.Token == Token && x.IsActive == true && x.RoleGroupID == 1);

            //if (user != null)
            //{
            //    issucces = true;
            //    message = "Login Success";
            //}

            //var a = Db.AddLog("Api", "ServiceLogin", user.Username, user.ID, "", issucces, message, GetIPAddress(), DateTime.Now, user.ID);

            return issucces;
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