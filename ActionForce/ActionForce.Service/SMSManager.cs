using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Service
{
    public class SMSManager
    {

        public const string apiUrl = "http://websms.telsam.com.tr";
        public const string ApiToken = "f28f1eab60822970df1ad6db1474b7bb";
        public const string ApiKeyCode = "e89463d6a7e180766281c0c3889e9380";
        public const string Originator = "UFE GRUP AS";

        public const string ApiUser = "apiuser";
        public const string ApiPassword = "n4q4n6U0";


        private RestClient Client = new RestClient(apiUrl);

        public SMSManager()
        {
            Client.AddDefaultHeader("cache-control", "no-cache");
            Client.AddDefaultHeader("accept", "application/xml; charset=UTF-8");
            Client.AddDefaultHeader("content-type", "application/xml; charset=UTF-8");
        }
        public bool SendSMS(string Message, string PhoneNumber, bool? IsInternational)
        {
            bool issend = false;

            var isInternational = IsInternational == true ? "1" : "";
            string xmlbody = $"<?xml version=\"1.0\"?><SMS><authentication><token>{ApiToken}</token><keycode>{ApiKeyCode}</keycode></authentication><message><originator>{Originator}</originator><text>{Message}</text><unicode>1</unicode><international>{isInternational}</international><canceltext></canceltext><phonetrim></phonetrim></message><receivers><receiver>{PhoneNumber}</receiver></receivers></SMS>";
            var request = new RestRequest("xmlapiV4/sendsms", Method.POST, DataFormat.Xml);
            request.AddHeader("X-Requested-With", "RestSharp");
            request.AddParameter("text/xml", xmlbody, ParameterType.RequestBody);

            var response = Client.Execute(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                issend = true;
            }

            return issend;
        }

        //public void SendSMSOld(string Message, string PhoneNumber, bool? IsInternational)
        //{
        //    var isInternational = IsInternational == true ? "1" : "";
        //    string xmlbody = $"<?xml version='1.0'?><SMS><authentication><username>{ApiUser}</username><password>{ApiPassword}</password></authentication><message><originator>{Originator}</originator><text>{Message}</text><unicode>1</unicode><international>{isInternational}</international><canceltext></canceltext></message><receivers><receiver>{PhoneNumber}</receiver></receivers></SMS>";
        //    var request = new RestRequest("xmlapiV4/sendsms", Method.POST);
        //    request.AddXmlBody(xmlbody);

        //    var response = Client.Execute(request);
        //    if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {

        //    }

        //}

    }
}

