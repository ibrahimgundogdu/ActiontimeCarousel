using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.CardService.Models
{
    public class ServiceDataHelper
    {
        public static Employee ProcessEmployee(Employee employee)
        {

            var isimarray = employee.FullName.Split(' ').ToArray();

            if (isimarray.Length == 5)
            {
                employee.Name = isimarray[0] + " " + isimarray[1] + " " + isimarray[2] + " " + isimarray[3].Trim();
                employee.Surname = isimarray[4].Trim();
            }

            if (isimarray.Length == 4)
            {
                employee.Name = isimarray[0] + " " + isimarray[1] + " " + isimarray[2].Trim();
                employee.Surname = isimarray[3].Trim();
            }

            if (isimarray.Length == 3)
            {
                employee.Name = isimarray[0] + " " + isimarray[1].Trim();
                employee.Surname = isimarray[2].Trim();
            }

            if (isimarray.Length == 2)
            {
                employee.Name = isimarray[0].Trim();
                employee.Surname = isimarray[1].Trim();
            }

            if (isimarray.Length == 1)
            {
                employee.Name = isimarray[0].Trim();
                employee.Surname = isimarray[0].Trim();
            }

            if (isimarray.Length == 0)
            {
                employee.Name = "No";
                employee.Surname = "Name";
            }


            employee.Name = employee.Name.ToUpper().Replace('Ğ', 'G').Replace('Ü', 'U').Replace('İ', 'I').Replace('Ö', 'O').Replace('Ç', 'C');
            employee.Surname = employee.Surname.ToUpper().Replace('Ğ', 'G').Replace('Ü', 'U').Replace('İ', 'I').Replace('Ö', 'O').Replace('Ç', 'C');

            return employee;
        }
    }
}