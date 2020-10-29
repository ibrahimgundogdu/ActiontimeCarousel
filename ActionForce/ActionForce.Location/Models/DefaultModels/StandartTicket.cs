using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Location
{
    public class StandartTicket : LayoutControlModel
    {
        public VPrice Price { get; set; }
        public int PayMethodID { get; set; }
        public int EmployeeID { get; set; }
        public IEnumerable<AnimalCostume> AnimalCostumes { get; set; }
        public IEnumerable<MallMotoColor> MallMotoColor { get; set; }

    }
}