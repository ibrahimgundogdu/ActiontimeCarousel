using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation
{
    public class FormCardReader
    {
        public int CardReaderID { get; set; }
        public int CurrentCardReaderID { get; set; }
        public int LocationID { get; set; }
        public int LocationTypeID { get; set; }
        public int CardReaderTypeID { get; set; }
        public int LocationPartID { get; set; }
    }
}