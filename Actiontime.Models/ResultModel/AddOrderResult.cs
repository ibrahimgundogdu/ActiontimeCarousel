using Actiontime.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.ResultModel
{
    public class AddOrderResult
    {
        public int id { get; set; }
        public string orderNumber { get; set; }
        public string uid { get; set; }
        public double total { get; set; }
        public string currency { get; set; }
        public int printCount { get; set; }
    }
}
