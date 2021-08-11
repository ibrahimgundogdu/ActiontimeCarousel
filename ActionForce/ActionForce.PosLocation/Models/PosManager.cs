using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class PosManager
    {
        public List<DataEmployee> GetLocationEmployeesToday(int LocationID)
        {

            List<DataEmployee> EmployeeList = new List<DataEmployee>();

            using (ActionTimeEntities _db = new ActionTimeEntities())
            {
                var location = _db.Location.FirstOrDefault(x => x.LocationID == LocationID);
                var employeeList = _db.GetLocationEmployees(LocationID).Where(x => x.Active == true).ToList();
                var schedules = _db.Schedule.Where(x => x.LocationID == LocationID && x.ShiftDate == location.LocalDate).ToList();

                EmployeeList = employeeList.Select(x => new DataEmployee()
                {
                    LocationID = LocationID,
                    EmployeeID = x.EmployeeID,
                    EmployeeFullname = x.FullName,
                    IsScheduled = schedules.Any(y => y.EmployeeID == x.EmployeeID),
                    PhotoFile = x.PhotoFile
                }).ToList();

            }
            return EmployeeList;
        }
    }
}