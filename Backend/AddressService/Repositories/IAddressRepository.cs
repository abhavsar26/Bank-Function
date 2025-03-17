using AddressService.Models;

namespace AddressService.Repositories
{
    public interface IAddressRepository
    {
        Task<Address> GetAddressByIdAsync(int addressId);
        Task<IEnumerable<Address>> GetAddressesByCustomerIdAsync(int customerId);
        Task AddAddressAsync(Address address);
        Task UpdateAddressAsync(Address address);
        Task DeleteAddressAsync(int addressId);
    }
}
