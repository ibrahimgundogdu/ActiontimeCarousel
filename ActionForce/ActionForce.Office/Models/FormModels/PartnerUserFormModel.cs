using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class PartnerUserFormModel
    {
        public Guid PartnerUID { get; set; }
        public int PartnerUserID { get; set; }
        public string UserFullname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Password2 { get; set; }
        public string Email { get; set; }
        public string SubmitUpdate { get; set; }
        public string SubmitRemove { get; set; }
    }
}