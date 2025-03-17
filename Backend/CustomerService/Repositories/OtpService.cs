namespace CustomerService.Repositories
{
    public class OtpService
    {
        private readonly Dictionary<string, string> _otpStore = new();

        public void SaveOtp(string phoneNumber, string otp)
        {
            _otpStore[phoneNumber] = otp;
        }

        public bool VerifyOtp(string otp)
        {
            return _otpStore.Values.Any(o => o == otp);
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }
    }
}
