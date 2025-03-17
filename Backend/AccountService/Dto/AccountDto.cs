namespace AccountService.Dto
{
    public class AccountDto
    {
        public int AccountId { get; set; }

        public int CustomerId { get; set; }

        public string? AccountType { get; set; }

        public string AccountNumber { get; set; } = null!;

        public string? Category { get; set; }

        public string? JointAccountHolderName { get; set; }

        public string? Status { get; set; }

        public DateTime DateOpened { get; set; }

        public double? InterestRate { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public decimal? Balance { get; set; }

    }
}
