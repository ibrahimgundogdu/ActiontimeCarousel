using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class AddDocumentParameters
    {
        public int employeeId { get; set; }
        public string docDate { get; set; }
        public string amount { get; set; }
        public string description { get; set; }
        public int docType { get; set; }

    }
}
