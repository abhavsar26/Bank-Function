using CustomerService.Dto;
using CustomerService.Models;
using CustomerService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher<Customer> _passwordHasher; // Password hasher
        private readonly OtpService _otpService;
        private readonly CustomerBankContext _dbContext;
        private readonly SmsService _smsService;
        public CustomersController(ICustomerRepository customerRepository, OtpService otpService, SmsService smsService)
        {
            _customerRepository = customerRepository;
            _passwordHasher = new PasswordHasher<Customer>(); // Initialize password hasher
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        }

        // GET: api/customers
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAllCustomers()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerById(int id)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        // POST: api/customers
        //[Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Hash the password before saving
            customer.PasswordHash = _passwordHasher.HashPassword(customer, customer.PasswordHash); // Hashing password

            // Hash the security answer before saving, if provided
            if (!string.IsNullOrEmpty(customer.SecurityQuestion) && !string.IsNullOrEmpty(customer.SecurityAnswerHash))
            {
                customer.SecurityAnswerHash = _passwordHasher.HashPassword(customer, customer.SecurityAnswerHash); // Hashing security answer
            }

            await _customerRepository.AddCustomerAsync(customer);

            return CreatedAtAction(nameof(GetCustomerById), new { id = customer.CustomerId }, customer);
        }

        // PUT: api/customers/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest("Customer ID mismatch");
            }

            var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Optionally, re-hash password if updated
            if (!string.IsNullOrEmpty(customer.PasswordHash))
            {
                customer.PasswordHash = _passwordHasher.HashPassword(customer, customer.PasswordHash);
            }

            // Hash the security answer if updated
            if (!string.IsNullOrEmpty(customer.SecurityAnswerHash))
            {
                customer.SecurityAnswerHash = _passwordHasher.HashPassword(customer, customer.SecurityAnswerHash);
            }

            await _customerRepository.UpdateCustomerAsync(customer);
            return NoContent();
        }
        [HttpGet("username/{username}")]
        public async Task<ActionResult<Customer>> GetCustomerByUsername(string username)
        {
            var customer = await _customerRepository.GetCustomerByUsernameAsync(username);
            if (customer == null)
            {
                return NotFound();
            }
            return Ok(customer);
        }

        // DELETE: api/customers/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var existingCustomer = await _customerRepository.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            await _customerRepository.DeleteCustomerAsync(id);
            return NoContent();
        }
        //[HttpPost("reset-password")]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        //{
        //    // Find the customer by email
        //    var customer = await _customerRepository.GetCustomerByEmailAsync(request.Email);
        //    if (customer == null)
        //    {
        //        return NotFound(new { message = "Customer not found" });
        //    }

        //    // Update the password (make sure to hash it)
        //    customer.PasswordHash = _passwordHasher.HashPassword(customer, request.NewPassword);

        //    // Update the customer record in the database
        //    await _customerRepository.UpdateCustomerAsync(customer);

        //    return Ok(new { message = "Password has been reset successfully." });
        //}

        //[HttpPost("reset-password")]
        //public IActionResult ResetPassword([FromBody] Dto.ResetPasswordRequest request)
        //{
        //    var customer = _dbContext.Customers.FirstOrDefault(c => c.PhoneNumber == request.PhoneNumber);
        //    if (customer == null)
        //    {
        //        return NotFound(new { message = "Customer not found." });
        //    }

        //    customer.PasswordHash = _passwordHasher.HashPassword(customer, request.NewPassword);
        //    _dbContext.SaveChanges();

        //    return Ok(new { message = "Password updated successfully." });
        //}

        //// POST: api/customers/send-otp
        //[HttpPost("send-otp")]
        //public async Task<IActionResult> SendOtp([FromBody] OtpRequest request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest(new { message = "Request cannot be null." });
        //    }

        //    if (string.IsNullOrEmpty(request.PhoneNumber))
        //    {
        //        return BadRequest(new { message = "Phone number is required." });
        //    }

        //    string otp = GenerateOtp();
        //    _otpService.SaveOtp(request.PhoneNumber, otp);

        //    try
        //    {
        //        await _smsService.SendSmsAsync(request.PhoneNumber, $"Your OTP is: {otp}");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to send OTP.", error = ex.Message });
        //    }

        //    return Ok(new { message = "OTP sent successfully." });
        //}

        //// POST: api/customers/verify-otp
        //[HttpPost("verify-otp")]
        //public IActionResult VerifyOtp([FromBody] OtpVerificationRequest request)
        //{
        //    bool isValid = _otpService.VerifyOtp(request.Otp);

        //    if (!isValid)
        //    {
        //        return BadRequest(new { message = "Invalid OTP." });
        //    }

        //    return Ok(new { message = "OTP verified." });
        //}

        //private string GenerateOtp()
        //{
        //    // Logic to generate a random OTP (e.g., 6-digit code)
        //    return new Random().Next(100000, 999999).ToString();
        //}
        // In CustomersController.cs

        // POST: api/customers/forgot-password
        // POST: api/customers/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            // Validate the input model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find the customer by email
            var customer = await _customerRepository.GetCustomerByEmailAsync(forgotPasswordDto.Email);
            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            // Check if the security question matches
            if (customer.SecurityQuestion != forgotPasswordDto.SecurityQuestion)
            {
                return BadRequest("Security question does not match.");
            }

            // Verify the security answer
            var answerVerification = _passwordHasher.VerifyHashedPassword(customer, customer.SecurityAnswerHash, forgotPasswordDto.SecurityAnswerHash);
            if (answerVerification == PasswordVerificationResult.Failed)
            {
                return BadRequest("Security answer is incorrect.");
            }

            // Hash the new password
            customer.PasswordHash = _passwordHasher.HashPassword(customer, forgotPasswordDto.NewPassword);

            // Update the customer in the database
            await _customerRepository.UpdateCustomerAsync(customer);

            return Ok(new { message = "Password updated successfully." });
        }
        [HttpGet("dto/{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerDtoById(int id)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            // Map Customer to CustomerDto
            var customerDto = new CustomerDto
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                DateOfBirth = customer.DateOfBirth
            };

            return Ok(customerDto);
        }
        [HttpPut("dto/{id}")]
        public async Task<ActionResult> UpdateCustomer(int id, [FromBody] CustomerDto updateCustomerDto)
        {
            if (updateCustomerDto == null)
            {
                return BadRequest("Customer data is required.");
            }

            // Retrieve the existing customer
            var customer = await _customerRepository.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            // Update customer properties
            customer.FirstName = updateCustomerDto.FirstName ?? customer.FirstName;
            customer.LastName = updateCustomerDto.LastName ?? customer.LastName;
            customer.Email = updateCustomerDto.Email ?? customer.Email;
            customer.PhoneNumber = updateCustomerDto.PhoneNumber ?? customer.PhoneNumber;
            customer.DateOfBirth = updateCustomerDto.DateOfBirth ?? customer.DateOfBirth;

            // Save changes to the database
            await _customerRepository.UpdateCustomerAsync(customer);

            return NoContent(); // Return 204 No Content on success
        }


    }
}
