using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionForce.Integration.UfeService
{
    public class UfeServiceClient
    {
        public const string apiUrl = "http://apiservice.ufegrup.com";

        private RestClient Client = new RestClient(apiUrl);

        public UfeServiceClient(string Token)
        {
            Client.AddDefaultHeader("cache-control", "no-cache");
            Client.AddDefaultHeader("accept", "application/json; charset=UTF-8");
            Client.AddDefaultHeader("content-type", "application/json; charset=UTF-8");
            Client.AddDefaultHeader("authorization", $"Basic {Token}");
        }

        public LocationShiftResult LocationShiftStart(int locationID, int? environmentID, double? latitude, double? longitude, string _Date)
        {
            var request = new RestRequest("Location/ShiftStart", Method.POST);
            request.AddJsonBody(new { LocationID = locationID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute<LocationShiftResult>(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        public LocationShiftResult LocationShiftEnd(int locationID, int? environmentID, double? latitude, double? longitude, string _Date)
        {
            var request = new RestRequest("Location/ShiftEnd", Method.POST);
            request.AddJsonBody(new { LocationID = locationID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute<LocationShiftResult>(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        public EmployeeShiftResult EmployeeShiftStart(int locationID, int employeeID, int? environmentID, double? latitude, double? longitude, string _Date)
        {
            var request = new RestRequest("Employee/ShiftStart", Method.POST);
            request.AddJsonBody(new { LocationID = locationID, EmployeeID = employeeID,  EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute<EmployeeShiftResult>(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        public EmployeeShiftResult EmployeeShiftEnd(int locationID, int employeeID, int? environmentID, double? latitude, double? longitude, string _Date)
        {
            var request = new RestRequest("Employee/ShiftEnd", Method.POST);
            request.AddJsonBody(new { LocationID = locationID, EmployeeID = employeeID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute<EmployeeShiftResult>(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        public EmployeeShiftResult EmployeeBreakStart(int locationID, int employeeID, int? environmentID, double? latitude, double? longitude, string _Date)
        {
            var request = new RestRequest("Employee/BreakStart", Method.POST);
            request.AddJsonBody(new { LocationID = locationID, EmployeeID = employeeID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute<EmployeeShiftResult>(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

        public EmployeeShiftResult EmployeeBreakEnd(int locationID, int employeeID,int? environmentID, double? latitude, double? longitude, string _Date)
        {
            var request = new RestRequest("Employee/BreakEnd", Method.POST);
            request.AddJsonBody(new { LocationID = locationID, EmployeeID = employeeID, EnvironmentID = environmentID, Latitude = latitude, Longitude = longitude, Date = _Date });
            var response = Client.Execute<EmployeeShiftResult>(request);
            if (response.ResponseStatus == ResponseStatus.Completed && response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return response.Data;
            }
            return null;
        }

    }
}
