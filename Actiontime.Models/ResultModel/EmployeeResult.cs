using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.ResultModel
{
    public class EmployeeResult : Result
    {
        public Employee? Employee { get; set; }
    }
}
