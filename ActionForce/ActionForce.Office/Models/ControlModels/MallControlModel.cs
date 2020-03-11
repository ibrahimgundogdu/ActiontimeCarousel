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
        public Country RelatedCountry { get; set; }
        public List<State> StateList { get; set; }
        public List<City> CityList { get; set; }
        public List<County> CountyList { get; set; }
        public List<VCity> ExistingCities { get; set; }
        public List<OurCompany> OurCompanyList { get; set; }
        public List<MallSegment> MallSegmentList { get; set; }
        public Result Result { get; set; }
        public MallFilterModel FilterModel { get; set; }
        public FormMall CheckMall { get; set; }
    }
}