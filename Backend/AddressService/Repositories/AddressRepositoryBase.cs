using AddressService.Models;

namespace AddressService.Repositories
{
    public abstract class AddressRepositoryBase:IAddressRepository
    {
        protected readonly AddressBankContext _context;

        protected AddressRepositoryBase(AddressBankContext context)
        {
            _context = context;
        }
        public abstract Task<Address> GetAddressByIdAsync(int addressId);
        public abstract Task<IEnumerable<Address>> GetAddressesByCustomerIdAsync(int customerId);
        public abstract Task AddAddressAsync(Address address);
        public abstract Task UpdateAddressAsync(Address address);
        public abstract Task DeleteAddressAsync(int addressId);
    }
}
