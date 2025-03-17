using AccountService.Models;

namespace AccountService.Repositories
{
    public interface IAccountRepository
    {
        Task<Account> GetAccountByAccountNumberAsync(int accountId);
        Task<IEnumerable<Account>> GetAccountsByCustomerIdAsync(int customerId);
        Task AddAccountAsync(Account account);
        Task UpdateAccountAsync(Account account);
        Task DeleteAccountAsync(int accountId);
        Task<Account> GetAccountByAccountNumberAsync(string accountNumber);
        Task<Account?> GetAccountByIdAsync(int accountId);
    }
}
