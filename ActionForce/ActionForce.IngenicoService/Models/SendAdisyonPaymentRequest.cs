using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class SendAdisyonPaymentRequest
    {

        public Header Header_Info { get; set; }
        public long AdisyonId { get; set; }
        public Receipt Receipt { get; set; }
        public string SerialNo { get; set; }
    }
}