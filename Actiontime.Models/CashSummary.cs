namespace Actiontime.Models
{
    public class CashSummary
    {
        public double CashBlockedAmount { get; set; }
        public double CashAmount { get; set; }
        public double CreditAmount { get; set; }
        public double ExpenseAmount { get; set; }
        public double LaborAmount { get; set; }
        public double SaleAmount { get; set; }

        public double saleCashAmount { get; set; }
        public double saleCreditAmount { get; set; }

        public double CashBalance { get; set; }
        public double CreditBalance { get; set; }
        public double CashTransferBalance { get; set; }
        public double CreditTransferBalance { get; set; }
        public double NetBalance { get; set; }

        public double RefundAmount { get; set; }
        public double RefundCashAmount { get; set; }
        public double RefundCreditAmount { get; set; }
    }
}