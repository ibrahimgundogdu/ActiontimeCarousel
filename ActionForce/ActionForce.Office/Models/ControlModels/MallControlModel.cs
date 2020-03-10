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
        public Result Result { get; set; }
    }
}