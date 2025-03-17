using CustomerService.Models;

namespace CustomerService.Repositories
{
    public abstract class CustomerRepositoryBase:ICustomerRepository
    {
        protected readonly CustomerBankContext _context;
        protected CustomerRepositoryBase(CustomerBankContext context)
        {
            _context = context;
        }
        public abstract Task AddCustomerAsync(Customer customer);
        public abstract Task DeleteCustomerAsync(int customerId);
        public abstract Task<IEnumerable<Customer>> GetAllCustomersAsync();
        public abstract Task<Customer> GetCustomerByIdAsync(int customerId);
        public abstract Task UpdateCustomerAsync(Customer customer);
        public abstract Task<Customer> GetCustomerByUsernameAsync(string username);
        public abstract Task<Customer> GetCustomerByEmailAsync(string email);
    }
}
