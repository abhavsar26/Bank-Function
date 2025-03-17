using System.ComponentModel.DataAnnotations;

namespace AccountService.Dto
{
    public class TransferDto
    {
        [Required]
        public string SourceAccountNumber { get; set; }

        [Required]
        public string DestinationAccountNumber { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

    }
}
