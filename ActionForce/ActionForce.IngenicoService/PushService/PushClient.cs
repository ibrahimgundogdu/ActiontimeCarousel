using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class PushClient
    {
        public const string apiUrl = "http://push.ufegrup.com";

        private RestClient Client = new RestClient(apiUrl);

        public PushClient() //string Token
        {
            Client.AddDefaultHeader("cache-control", "no-cache");
            Client.AddDefaultHeader("accept", "application/json; charset=UTF-8");
            Client.AddDefaultHeader("content-type", "application/json; charset=UTF-8");
            //Client.AddDefaultHeader("authorization", $"Basic {Token}");
        }

        public bool SendMessage(string SerialNumber, string Message)
        {
            var request = new RestRequest("Messenger/SendMessage/?message="+Message+"&serial="+SerialNumber, Method.GET);
            //request.AddJsonBody(new { LocationID = locationID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

    }
}