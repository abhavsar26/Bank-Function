using AddressService.Models;
using Microsoft.EntityFrameworkCore;

namespace AddressService.Repositories
{
    public class AddressRepository:AddressRepositoryBase
    {
        public AddressRepository(AddressBankContext context) : base(context)
        {
        }

        public override async Task<Address> GetAddressByIdAsync(int addressId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AddressId == addressId);
        }

        public override async Task<IEnumerable<Address>> GetAddressesByCustomerIdAsync(int customerId)
        {
            return await _context.Addresses
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public override async Task AddAddressAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
        }

        public override async Task UpdateAddressAsync(Address address)
        {
            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAddressAsync(int addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
        }
    }
}
