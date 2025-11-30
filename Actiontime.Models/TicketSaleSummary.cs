namespace Actiontime.Models
{
    public class TicketSaleSummary
    {
        public string PaymentType { get; set; }
        public string TicketName { get; set; }
        public string StatusName { get; set; }
        public int Unit { get; set; }
        public double UnitPrice { get; set; }
        public int SaleCount { get; set; }
        public double SaleAmount { get; set; }
    }
}