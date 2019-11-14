using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Integration
{
    public class TCMBClient
    {
        public const string apiKey = "2EygT4N3mj";
        public const string apiUrl = "https://evds2.tcmb.gov.tr/service/evds";

        private RestClient Client = new RestClient(apiUrl);

        public TcmbKurlar GetExchangeToday(string date)
        {
            string urlParam = $"series=TP.DK.USD.A-TP.DK.EUR.A-TP.DK.USD.S-TP.DK.EUR.S&startDate={date}&endDate={date}&type=json&key={apiKey}";
            var request = new RestRequest(urlParam, Method.GET);
            var response = Client.Execute<TcmbKurlar>(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<TcmbKurlar>(response.Content); ;
            }
            return null;

        }

       
    }
}
