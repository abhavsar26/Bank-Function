namespace AccountService.Dto
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = null!;
        public string? Description { get; set; }
        public string AccountNumber { get; set; } = null!;
    }
}
