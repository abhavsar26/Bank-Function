using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories
{
    public class TransactionRepository:ITransactionRepository
    {
        private readonly AccountBankContext _context;

        public TransactionRepository(AccountBankContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }
        public async Task<Transaction> GetLatestTransactionByDescriptionAsync(int accountId, string description)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId && t.Description.Contains(description))
                .OrderByDescending(t => t.TransactionDate)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
