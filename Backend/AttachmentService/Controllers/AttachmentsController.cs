
using AttachmentService.Dto;
using AttachmentService.Models;
using AttachmentService.Repositories;
using CustomerService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace AccountService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IHttpClientFactory _httpClientFactory; // Use IHttpClientFactory
        private readonly string _customerServiceBaseUrl;

        public AttachmentsController(IAttachmentRepository attachmentRepository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _attachmentRepository = attachmentRepository;
            _httpClientFactory = httpClientFactory; // Initialize IHttpClientFactory
            _customerServiceBaseUrl = configuration.GetValue<string>("CustomerServiceBaseUrl");
        }

        // GET: api/attachments/{id}
        [Authorize] // Protects this endpoint, requires JWT token
        [HttpGet("{id}")]
        public async Task<ActionResult<Attachment>> GetAttachmentById(int id)
        {
            var attachment = await _attachmentRepository.GetAttachmentByIdAsync(id);
            if (attachment == null) return NotFound("Attachment not found.");

            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;
            try
            {
                // Make an HTTP request to the customer service to get the customer by their ID
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{attachment.CustomerId}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data.");
            }

            if (!customerResponse.IsSuccessStatusCode)
            {
                return NotFound("Customer not found.");
            }

            Customer customer;
            try
            {
                // Deserialize the customer data from the response
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            // Attach the customer information to the attachment object
            attachment.Customer = customer;

            // Return the enriched attachment with the customer information
            return Ok(attachment);
        }

        // GET: api/attachments/customer/{customerId}
        [Authorize] // Protect this endpoint
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachmentsByCustomerId(int customerId)
        {
            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;
            try
            {
                // Make an HTTP request to the customer service to get the customer by their ID
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{customerId}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data.");
            }

            if (!customerResponse.IsSuccessStatusCode)
            {
                return NotFound("Customer not found.");
            }

            Customer customer;
            try
            {
                // Deserialize the customer data from the response
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            // Fetch the attachments associated with the customer from the repository
            var attachments = await _attachmentRepository.GetAttachmentsByCustomerIdAsync(customerId);

            if (attachments == null || !attachments.Any())
            {
                return NotFound("Attachments not found for the customer.");
            }

            // Attach the customer data to each attachment
            foreach (var attachment in attachments)
            {
                attachment.Customer = customer;
            }

            // Return the attachments with customer data
            return Ok(attachments);
        }

        // POST: api/attachments
        //[Authorize] // Protect this endpoint
        [HttpPost]
        public async Task<ActionResult> AddAttachment(AttachmentDto attachmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map AttachmentDto to Attachment
            var attachment = new Attachment
            {
                AttachmentId = attachmentDto.AttachmentId,
                CustomerId = attachmentDto.CustomerId,
                AttachmentType = attachmentDto.AttachmentType,
                FilePath = attachmentDto.FilePath,
                CreatedAt = attachmentDto.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = attachmentDto.UpdatedAt ?? DateTime.UtcNow
            };

            await _attachmentRepository.AddAttachmentAsync(attachment);

            return CreatedAtAction(nameof(GetAttachmentById), new { id = attachment.AttachmentId }, attachmentDto);
        }

        // PUT: api/attachments/{id}
        [Authorize] // Protect this endpoint
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAttachment(int id, AttachmentDto attachmentDto)
        {
            if (id != attachmentDto.AttachmentId)
            {
                return BadRequest("Attachment ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingAttachment = await _attachmentRepository.GetAttachmentByIdAsync(id);
            if (existingAttachment == null)
            {
                return NotFound();
            }

            // Map AttachmentDto to Attachment
            var attachment = new Attachment
            {
                AttachmentId = attachmentDto.AttachmentId,
                CustomerId = attachmentDto.CustomerId,
                AttachmentType = attachmentDto.AttachmentType,
                FilePath = attachmentDto.FilePath,
                CreatedAt = existingAttachment.CreatedAt, // Preserve original creation date
                UpdatedAt = DateTime.UtcNow
            };

            await _attachmentRepository.UpdateAttachmentAsync(attachment);
            return NoContent();
        }

        // DELETE: api/attachments/{id}
        [Authorize] // Protect this endpoint
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAttachment(int id)
        {
            var existingAttachment = await _attachmentRepository.GetAttachmentByIdAsync(id);
            if (existingAttachment == null)
            {
                return NotFound();
            }

            await _attachmentRepository.DeleteAttachmentAsync(id);
            return Ok("Attachment deleted successfully");
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file, [FromForm] int customerId, [FromForm] string attachmentType)
        {
            if (string.IsNullOrWhiteSpace(attachmentType))
            {
                return BadRequest("Attachment type is required.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadsDirectory))
                Directory.CreateDirectory(uploadsDirectory);

            var filePath = Path.Combine(uploadsDirectory, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create new attachment entry
            var attachment = new Attachment
            {
                CustomerId = customerId,
                AttachmentType = attachmentType,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _attachmentRepository.AddAttachmentAsync(attachment);
            return Ok(new { message = "File uploaded successfully.", filePath });
        }

        // File Download endpoint
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            var attachment = await _attachmentRepository.GetAttachmentByIdAsync(id);
            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath))
                return NotFound("Attachment not found.");

            var filePath = attachment.FilePath;

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/octet-stream", Path.GetFileName(filePath));
        }
    }
}
