using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class QRCInfo
    {
        public string SerialNumber { get; set; }
        public int? Process { get; set; }
        public string QRCode { get; set; }
        public string ConfirmNumber { get; set; }
    }
}
