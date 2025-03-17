using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AddressService.Models;
using AddressService.Repositories;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using AddressService.Dto;
using CustomerService.Models;

namespace AddressService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressRepository _addressRepository;
        private readonly IHttpClientFactory _httpClientFactory; // Use IHttpClientFactory
        private readonly string _customerServiceBaseUrl;

        public AddressesController(IAddressRepository addressRepository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _addressRepository = addressRepository;
            _httpClientFactory = httpClientFactory; // Initialize IHttpClientFactory
            _customerServiceBaseUrl = configuration.GetValue<string>("CustomerServiceBaseUrl");
        }

        // GET: api/addresses/{id}
        [Authorize] // Protects this endpoint, requires JWT token
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddressById(int id)
        {
            var address = await _addressRepository.GetAddressByIdAsync(id);
            if (address == null) return NotFound("Address not found.");

            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;
            try
            {
                // Make an HTTP request to CustomerService to get customer details
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{address.CustomerId}");
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
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            address.Customer = customer; // Attach customer information to address

            return Ok(address);
        }

        // GET: api/addresses/customer/{customerId}
        [Authorize] // Secure endpoint
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddressesByCustomerId(int customerId)
        {
            var addresses = await _addressRepository.GetAddressesByCustomerIdAsync(customerId);
            if (addresses == null || !addresses.Any()) return NotFound("No addresses found for this customer.");

            var httpClient = _httpClientFactory.CreateClient(); // Create HttpClient
            HttpResponseMessage customerResponse;
            try
            {
                customerResponse = await httpClient.GetAsync($"{_customerServiceBaseUrl}/api/Customers/{customerId}");
            }
            catch (HttpRequestException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer data.");
            }

            if (!customerResponse.IsSuccessStatusCode) return NotFound("Customer not found.");

            Customer customer;
            try
            {
                customer = await customerResponse.Content.ReadFromJsonAsync<Customer>();
            }
            catch (JsonException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing customer data.");
            }

            foreach (var address in addresses)
            {
                address.Customer = customer; // Attach customer data to each address
            }

            return Ok(addresses);
        }

        // POST: api/addresses
        //[Authorize] // Protect endpoint
        [HttpPost]
        public async Task<ActionResult> AddAddress(AddressDto addressDto)
        {
            var address = new Address
            {
                AddressId = addressDto.AddressId,
                CustomerId = addressDto.CustomerId,
                HouseNumber = addressDto.HouseNumber,
                Street = addressDto.Street,
                City = addressDto.City,
                State = addressDto.State,
                Country = addressDto.Country,
                PinCode = addressDto.PinCode,
                CreatedAt = addressDto.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = addressDto.UpdatedAt ?? DateTime.UtcNow
            };

            await _addressRepository.AddAddressAsync(address);

            return CreatedAtAction(nameof(GetAddressById), new { id = address.AddressId }, addressDto);
        }

        // PUT: api/addresses/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAddress(int id, AddressDto addressDto)
        {
            if (id != addressDto.AddressId) return BadRequest("Address ID mismatch.");

            var existingAddress = await _addressRepository.GetAddressByIdAsync(id);
            if (existingAddress == null) return NotFound();

            var address = new Address
            {
                AddressId = addressDto.AddressId,
                CustomerId = addressDto.CustomerId,
                HouseNumber = addressDto.HouseNumber,
                Street = addressDto.Street,
                City = addressDto.City,
                State = addressDto.State,
                Country = addressDto.Country,
                PinCode = addressDto.PinCode,
                CreatedAt = existingAddress.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            await _addressRepository.UpdateAddressAsync(address);
            return NoContent();
        }

        // DELETE: api/addresses/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAddress(int id)
        {
            var existingAddress = await _addressRepository.GetAddressByIdAsync(id);
            if (existingAddress == null) return NotFound();

            await _addressRepository.DeleteAddressAsync(id);
            return Ok("Address deleted successfully");
        }
        [HttpGet("dto/customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<AddressPDto>>> GetAddressDtosByCustomerId(int customerId)
        {
            var addresses = await _addressRepository.GetAddressesByCustomerIdAsync(customerId);

            if (addresses == null || !addresses.Any())
            {
                return NotFound("No addresses found for this customer.");
            }

            // Map the addresses to AddressDto
            var addressDtos = addresses.Select(address => new AddressPDto
            {
                HouseNumber = address.HouseNumber,
                Street = address.Street,
                City = address.City,
                State = address.State,
                Country = address.Country,
                PinCode = address.PinCode
            }).ToList();

            return Ok(addressDtos);
        }
        [HttpPut("dto/customer/{customerId}/address")]
        public async Task<ActionResult> UpdateAddress(int customerId, [FromBody] AddressPDto addressUpdateDto)
        {
            if (addressUpdateDto == null)
            {
                return BadRequest("Address data is required.");
            }

            // Retrieve the existing address for the customer
            var addresses = await _addressRepository.GetAddressesByCustomerIdAsync(customerId);

            if (addresses == null || !addresses.Any())
            {
                return NotFound($"No addresses found for customer ID {customerId}.");
            }

            // Assuming we update the first address (you can modify this logic as needed)
            var addressToUpdate = addresses.First(); // Get the first address or modify this logic as necessary

            // Update address properties
            addressToUpdate.HouseNumber = addressUpdateDto.HouseNumber;
            addressToUpdate.Street = addressUpdateDto.Street;
            addressToUpdate.City = addressUpdateDto.City;
            addressToUpdate.State = addressUpdateDto.State;
            addressToUpdate.Country = addressUpdateDto.Country;
            addressToUpdate.PinCode = addressUpdateDto.PinCode;

            // Save changes to the database
            await _addressRepository.UpdateAddressAsync(addressToUpdate);

            return NoContent(); // Return 204 No Content on success
        }

    }
}
