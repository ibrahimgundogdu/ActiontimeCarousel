using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.PosLocation.Models.DataModel
{
    public class EnvelopeCheck
    {
        public string Description { get; set; }
        public bool IsSuccess { get; set; }
        public ImportanceLevel Importance { get; set; }
    }
}