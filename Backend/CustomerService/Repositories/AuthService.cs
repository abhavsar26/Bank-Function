using CustomerService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace CustomerService.Repositories
{
    public class AuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher<Customer> _passwordHasher;
        private readonly string _jwtSecret;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(ICustomerRepository customerRepository, string jwtSecret, string issuer, string audience)
        {
            _customerRepository = customerRepository;
            _passwordHasher = new PasswordHasher<Customer>();
            _jwtSecret = jwtSecret;
            _issuer = issuer;
            _audience = audience;
        }

        public async Task<(string Token, int CustomerId)> LoginAsync(string username, string password)
        {
            var customer = await _customerRepository.GetCustomerByUsernameAsync(username);
            if (customer == null || _passwordHasher.VerifyHashedPassword(customer, customer.PasswordHash, password) == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, customer.Username),
                new Claim(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, _audience)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (jwtToken, customer.CustomerId); // Return both token and CustomerId
        }

        public async Task SendPasswordResetLinkAsync(string email)
        {
            var customer = await _customerRepository.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                throw new Exception("User with this email does not exist.");
            }

            var token = GeneratePasswordResetToken(customer);
            var resetLink = $"http://yourfrontend.com/reset-password?token={token}";

            await SendEmailAsync(email, "Password Reset", $"Please reset your password by clicking here: {resetLink}");
        }

        private string GeneratePasswordResetToken(Customer customer)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(customer.CustomerId.ToString()));
        }

        private async Task SendEmailAsync(string email, string subject, string body)
        {
            // Configure your SMTP server details here
            var smtpServer = "smtp.gmail.com"; // e.g., smtp.gmail.com
            var smtpPort = 587; // Usually 587 for TLS or 465 for SSL
            var smtpUser = "your-email@gmail.com"; // Your email
            var smtpPassword = "your-email-password"; // Your email password

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = true; // Enable SSL for secure connection
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword); // Set your credentials

                using (var message = new MailMessage(smtpUser, email)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // Set to true if you're sending HTML content
                })
                {
                    await client.SendMailAsync(message); // Send the email
                }
            }
        }
    }
}
