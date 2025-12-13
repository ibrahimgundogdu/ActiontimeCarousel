using Actiontime.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Services
{
    public class ServiceHelper : IServiceHelper
    {
        public static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
                return Convert.ToHexString(result);
            }
        }

        public static string UploadToFtp(byte[] File, string FileName, string FilePath)
        {

            try
            {

                var uploadurl = "ftp://document.actiontime.us";
                var uploadfilename = FileName;
                var username = "actiontimeftp";
                var password = "7C242B8A6C464D8FB8F553FDA850D24D!";
                byte[] buffer = File;
                string ftpurl = $"{uploadurl}/{FilePath}/{uploadfilename}";
                var requestObj = FtpWebRequest.Create(ftpurl) as FtpWebRequest;
                requestObj.Method = WebRequestMethods.Ftp.UploadFile;
                requestObj.Credentials = new NetworkCredential(username, password);
                Stream requestStream = requestObj.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Flush();
                requestStream.Close();
                requestObj = null;
            }
            catch (Exception ex)
            {
            }

            return FileName;
        }


    }
}
