using AccountService.Models;

namespace AccountService.Repositories
{
    public abstract class AccountRepositoryBase:IAccountRepository
    {
        protected readonly AccountBankContext _context;

        protected AccountRepositoryBase(AccountBankContext context)
        {
            _context = context;
        }

        public abstract Task<Account> GetAccountByAccountNumberAsync(int accountId);
        public abstract Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId);
        public abstract Task AddAccountAsync(Account account);
        public abstract Task UpdateAccountAsync(Account account);
        public abstract Task DeleteAccountAsync(int accountId);
        public abstract Task<Account> GetAccountByAccountNumberAsync(string accountNumber);
        public abstract Task<Account?> GetAccountByIdAsync(int accountId);
    }
}
