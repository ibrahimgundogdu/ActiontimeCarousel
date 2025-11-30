using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models
{
    public class OrderMqModel
    {
        public int OrderId { get; set; }
        public int? OrderRowId { get; set; }
        public OrderState State { get; set; }
    }
}
