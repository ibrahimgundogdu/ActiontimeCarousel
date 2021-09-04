using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class InventoryControlModel : LayoutControlModel
    {
        public IEnumerable<PosTerminalDataModel> PosTerminals { get; set; }
        public PosTerminalDataModel PosTerminal { get; set; }

        public IEnumerable<LocationDataModel> Locations { get; set; }

        public IEnumerable<BankDataModel> Banks { get; set; }

        public IEnumerable<LocationPosTerminal> LocationPosTerminals { get; set; }
        public IEnumerable<VLocationPosTerminal> VLocationPosTerminals { get; set; }

        public Result Result { get; set; }
        public PosFilterModel FilterModel { get; set; }



    }
}