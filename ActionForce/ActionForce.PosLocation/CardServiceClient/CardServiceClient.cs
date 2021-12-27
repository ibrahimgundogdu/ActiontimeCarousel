using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class CardServiceClient
    {
        public const string apiUrl = "http://nfcservice.ufegrup.com/api";
        //public const string apiUrl = "https://localhost:44350/api";

        private RestClient Client = new RestClient(apiUrl);

        public CardServiceClient() //string Token
        {
            Client.AddDefaultHeader("cache-control", "no-cache");
            Client.AddDefaultHeader("accept", "application/json; charset=UTF-8");
            Client.AddDefaultHeader("content-type", "application/json; charset=UTF-8");
            //Client.AddDefaultHeader("authorization", $"Basic {Token}");
        }

        public string CardLoad(Guid guid, int success, string Message)
        {
            var request = new RestRequest("CardLoad/CardLoadResult", Method.GET);
            request.AddParameter("id", guid);
            request.AddParameter("success", success);
            request.AddParameter("Message", Message);
            //request.AddJsonBody(new { LocationID = locationID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Yükleme Tamamlandı";
            }
            return "Yükleme Sorunu";
        }

    }
}