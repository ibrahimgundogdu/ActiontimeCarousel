using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class InventoryControlModel : LayoutControlModel
    {
        public IEnumerable<PosTerminal> PosTerminals { get; set; }
    }
}