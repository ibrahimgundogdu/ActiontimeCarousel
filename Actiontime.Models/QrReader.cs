using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class QrReader
    {

        public int Id { get; set; }
        public int? OurCompanyID { get; set; }
        public int? LocationID { get; set; }
        public int? LocationTypeID { get; set; }
        public int? QRReaderTypeID { get; set; }
        public int? LocationPartID { get; set; }
        public string PartName { get; set; }
        public string PartGroupName { get; set; }
        public string SerialNumber { get; set; }
        public string MACAddress { get; set; }
        public string Version { get; set; }
        public string IPAddress { get; set; }
        public int? DurationTime { get; set; }
        public int? TriggerCount { get; set; }
        public int? TriggerTime { get; set; }
        public Guid? UID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? IsActive { get; set; }


        public QrReader? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<QrReader>(json);
        }

        public string ToJson(QrReader reader)
        {
            return JsonConvert.SerializeObject(reader);
        }

    }


}
