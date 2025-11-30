using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Actiontime.Models.SerializeModels
{
    public class TicketReceipt
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Hour { get; set; }
        public string Date { get; set; }
        public string SaleID { get; set; }
        public string Counter { get; set; }
        public string FooterHeader { get; set; }
        public string FooterMessage { get; set; }
        public string SubTotal { get; set; }
        public string Discount { get; set; }
        public string Tax { get; set; }
        public string TotalTax { get; set; }
        public string Total { get; set; }
        public string Charge { get; set; }
        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string PrintCount { get; set; }

        public List<SaleRow> Rows { get; set; }
        public List<Ticket> Tickets { get; set; }

    }

    public class Ticket
    {
        public string TicketNumber { get; set; }
        public string TicketName { get; set; }
    }

    public class SaleRow
    {
        public string ItemName { get; set; }
        public string Price { get; set; }
    }
}
