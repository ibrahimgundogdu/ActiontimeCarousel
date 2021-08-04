using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosService
{
    public class UserMessageList
    {
        public string Data { get; set; }
        public int MessageType { get; set; }
        public int? fileName { get; set; } = null;
        public int MessageFont { get; set; }
        public bool IsBold { get; set; }
        public int Position { get; set; }
        public bool IsInverted { get; set; }
        public int TicketPosition { get; set; }

    }
}