using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class MallControlModel:LayoutControlModel
    {
        public List<VMall> MallList { get; set; }
        public List<MallLocationContract> LocationContracts { get; set; }
        public List<string> StateList { get; set; }
        public List<string> CityList { get; set; }
        public List<string> CountryList { get; set; }
        public Result Result { get; set; }
        public MallFilterModel FilterModel { get; set; }

    }
}