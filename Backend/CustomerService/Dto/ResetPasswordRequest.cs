namespace CustomerService.Dto
{
    public class ResetPasswordRequest
    {
        public string PhoneNumber { get; set; }
        public string NewPassword { get; set; }
    }
}
