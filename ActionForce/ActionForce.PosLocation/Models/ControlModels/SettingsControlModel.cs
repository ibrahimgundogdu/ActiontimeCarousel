using ActionForce.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class SettingsControlModel : LayoutControlModel
    {
        public List<CardReaderType> CardReaderTypes { get; set; }
        public List<LocationPart> LocationParts { get; set; }
        public List<VCardReader> CardReaders { get; set; }
        public List<VCardReader> NewCardReaders { get; set; }
        public VCardReader CardReader { get; set; }
        public VCardReader CurrentCardReader { get; set; }
        public List<NFCCardLog> NFCCardLogs { get; set; }
        public List<CardType> CardTypes { get; set; }

    }
}