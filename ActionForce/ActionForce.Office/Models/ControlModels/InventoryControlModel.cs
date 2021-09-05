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
        public LocationDataModel Location { get; set; }
        public IEnumerable<LocationDataModel> FilterLocations { get; set; }
        public IEnumerable<BankDataModel> Banks { get; set; }
        public IEnumerable<LocationPosTerminal> LocationPosTerminals { get; set; }
        public IEnumerable<VLocationPosTerminal> VLocationPosTerminals { get; set; }
        public Result Result { get; set; }
        public PosFilterModel FilterModel { get; set; }

        public IEnumerable<Costume> CostumeList { get; set; }
        public IEnumerable<CostumeType> CostumeTypeList { get; set; }
        public IEnumerable<VLocationAnimals> LocationAnimals { get; set; }
        public int? LocationID { get; set; }
        public int? CostumeTypeID { get; set; }


        public IEnumerable<MallMotoColor> MallMotoColors { get; set; }
        public IEnumerable<MallMoto> MallMotos { get; set; }
        public IEnumerable<VLocationMallMoto> LocationMallMotos { get; set; }
        public int? MallMotoColorID { get; set; }
    }
}