using ActionForce.Entity;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation.Models.Dapper
{
    public class DataService
    {
        private string ConnectionString;
        public DataService()
        {
            ConnectionString = PosManager.GetConnectionString();
        }

        public Location GetLocation(int LocationID)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocationID };
                var sql = "SELECT * FROM Location Where LocationID = @LocationID";
                var location = connection.QueryFirstOrDefault<Location>(sql, parameters);
                return location;
            }
        }

        public Location GetLocation(string LocationUID)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocationUID };
                var sql = "SELECT * FROM Location Where LocationUID = @LocationUID";
                var location = connection.QueryFirstOrDefault<Location>(sql, parameters);
                return location;
            }
        }

        public Employee GetEmployee(int EmployeeID)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { EmployeeID };
                var sql = "SELECT * FROM Employee Where EmployeeID = @EmployeeID";
                var employee = connection.QueryFirstOrDefault<Employee>(sql, parameters);
                return employee;
            }
        }

        public Employee GetEmployee(string EmployeeUID)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { EmployeeUID };
                var sql = "SELECT * FROM Employee Where EmployeeUID = @EmployeeUID";
                var employee = connection.QueryFirstOrDefault<Employee>(sql, parameters);
                return employee;
            }
        }

        public DateList GetDateInfo(DateTime LocalDate)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocalDate };
                var sql = "SELECT * FROM [dbo].[DateList] Where DateKey = @LocalDate";
                var result = connection.QueryFirstOrDefault<DateList>(sql, parameters);
                return result;
            }
        }

        public DayResult GetDayResult(int LocationID, DateTime LocalDate)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocationID, LocalDate };
                var sql = "SELECT * FROM [dbo].[DayResult] Where [LocationID] = @LocationID and [Date] = @LocalDate ";
                var result = connection.QueryFirstOrDefault<DayResult>(sql, parameters);
                return result;
            }
        }

        public DayResult CreateDayResult(int LocationID, DateTime LocalDate)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocationID, LocalDate };
                var sql = "SELECT * FROM [dbo].[DayResult] Where [LocationID] = @LocationID and [Date] = @LocalDate ";
                var result = connection.QueryFirstOrDefault<DayResult>(sql, parameters);
                return result;
            }
        }

        public LocationSchedule GetLocationSchedule(int LocationID, DateTime ShiftDate)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocationID, ShiftDate };
                var sql = "SELECT LocationID, ShiftDate, ShiftDateStart, ShiftdateEnd, ShiftDuration FROM LocationSchedule Where LocationID = @LocationID and ShiftDate = @ShiftDate";
                var result = connection.QueryFirstOrDefault<LocationSchedule>(sql, parameters);
                return result;
            }
        }

        public LocationShift GetLocationShift(int LocationID, DateTime ShiftDate)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var parameters = new { LocationID, ShiftDate };
                var sql = "SELECT LocationID, ShiftDate, ShiftDateStart, ShiftDateFinish, ShiftDuration FROM LocationShift Where LocationID = @LocationID and ShiftDate = @ShiftDate";
                var result = connection.QueryFirstOrDefault<LocationShift>(sql, parameters);
                return result;
            }
        }









    }
}