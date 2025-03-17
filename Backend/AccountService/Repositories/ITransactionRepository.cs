using AccountService.Models;

namespace AccountService.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsByAccountIdAsync(int accountId);
        Task AddTransactionAsync(Transaction transaction);
        Task<Transaction> GetLatestTransactionByDescriptionAsync(int accountId, string description);
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
