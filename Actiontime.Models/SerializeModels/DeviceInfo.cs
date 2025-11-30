using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class DeviceInfo
    {
        public int Process { get; set; }
        public string SerialNumber { get; set; }
        public string Ver { get; set; }
        public string IP { get; set; }
        public int? DurationTime { get; set; }
        public int? TriggerTime { get; set; }

    }

	public class DrawerDeviceInfo
	{
		public int Process { get; set; }
		public string SerialNumber { get; set; }
		public string Ver { get; set; }
		public string IP { get; set; }

	}


}

//{
//"Process": 1001,
//"SerialNumber": "08D1F9CA1C64",
//"Ver": "1.04",
//"IP": "1862314176",
//"DurationTime": 180,
//"TriggerTime": 1
//}