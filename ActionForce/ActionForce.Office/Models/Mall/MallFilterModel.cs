using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class MallFilterModel
    {
        public string MallName { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public int IsActive { get; set; }
    }
}