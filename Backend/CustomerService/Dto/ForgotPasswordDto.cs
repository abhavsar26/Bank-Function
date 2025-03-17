namespace CustomerService.Dto
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = null!;
        public string SecurityQuestion { get; set; } = null!;
        public string SecurityAnswerHash { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
