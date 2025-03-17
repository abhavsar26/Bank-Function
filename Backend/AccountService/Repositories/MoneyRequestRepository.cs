using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories
{
    public class MoneyRequestRepository:IMoneyRequestRepository
    {
        private readonly AccountBankContext _context;
        public MoneyRequestRepository(AccountBankContext context)
        {
            _context = context;
        }
        public async Task AddMoneyRequestAsync(MoneyRequest moneyRequest)
        {
            _context.MoneyRequests.Add(moneyRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<MoneyRequest>> GetMoneyRequestsByAccountNumberAsync(string accountNumber)
        {
            return await _context.MoneyRequests
                .Where(mr => mr.FromAccountNumber == accountNumber || mr.ToAccountNumber == accountNumber)
                .ToListAsync();
        }

        public async Task<MoneyRequest> GetMoneyRequestByIdAsync(int id)
        {
            return await _context.MoneyRequests.FindAsync(id);
        }
        // In your MoneyRequestRepository or equivalent repository class
        public async Task<IEnumerable<MoneyRequest>> GetPendingRequestsByAccountNumbersAsync(IEnumerable<string> accountNumbers)
        {
            return await _context.MoneyRequests
                .Where(m => accountNumbers.Contains(m.ToAccountNumber) && m.Status == "Pending")
                .ToListAsync();
        }
        public async Task UpdateMoneyRequestAsync(MoneyRequest moneyRequest)
        {
            _context.MoneyRequests.Update(moneyRequest); // Mark the entity as modified
            await _context.SaveChangesAsync(); // Save changes to the database
        }

    }
}
