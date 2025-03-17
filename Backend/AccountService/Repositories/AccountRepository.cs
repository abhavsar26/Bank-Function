using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories
{
    public class AccountRepository: AccountRepositoryBase
    {
        public AccountRepository(AccountBankContext context) : base(context)
        {
        }

        public override async Task<Account> GetAccountByAccountNumberAsync(int accountId)
        {
            return await _context.Accounts
                .AsNoTracking() // Eager loading Customer details
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }

        public override async Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId)
        {
            return await _context.Accounts
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public override async Task AddAccountAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public override async Task UpdateAccountAsync(Account account)
        {
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAccountAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
            }
        }
        public override async Task<Account> GetAccountByAccountNumberAsync(string accountNumber)
        {
            return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }
        public override async Task<Account?> GetAccountByIdAsync(int accountId)
        {
            // Fetch the account by ID using the database context
            return await _context.Accounts
                .Include(a => a.Transactions) // Include transactions if needed
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
        }
    }
}
