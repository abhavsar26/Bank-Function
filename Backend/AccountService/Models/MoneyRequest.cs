namespace AccountService.Models
{
    public partial class MoneyRequest
    {
        public int MoneyRequestId { get; set; }
        public string FromAccountNumber { get; set; }
        public string ToAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // e.g., Pending, Completed, Rejected
    }
}
