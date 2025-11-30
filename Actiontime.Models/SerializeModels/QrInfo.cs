using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class QRInfo
    {
        public string SerialNumber { get; set; }
        public int? Process { get; set; }
        public string QRCode { get; set; }

    }

    public class ConfirmInfo
    {
       
        public string QRCode { get; set; }
        public string ConfirmNumber { get; set; }
        public string SerialNumber { get; set; }
        public int? Process { get; set; }
    }

    public class ConplateInfo
    {

        public string QRCode { get; set; }
        public string ConfirmNumber { get; set; }
        public string SerialNumber { get; set; }
        public int? Process { get; set; }
        public int Duration { get; set; }
    }
}