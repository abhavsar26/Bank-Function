namespace AccountService.Dto
{
    public class AccountSummaryDto
    {
        public string CustomerName { get; set; }
        public string AccountType { get; set; }
        public string AccountNumber { get; set; }
        public string Category { get; set; }
        public string JointAccountHolderName { get; set; }
        public string Status { get; set; }
        public decimal? Balance { get; set; }

    }
}
