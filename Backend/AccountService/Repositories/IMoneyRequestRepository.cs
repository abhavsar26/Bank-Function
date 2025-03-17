using AccountService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Repositories
{
    public interface IMoneyRequestRepository
    {
        Task AddMoneyRequestAsync(MoneyRequest moneyRequest);
        Task<IEnumerable<MoneyRequest>> GetMoneyRequestsByAccountNumberAsync(string accountNumber);
        Task<MoneyRequest> GetMoneyRequestByIdAsync(int id);
        Task<IEnumerable<MoneyRequest>> GetPendingRequestsByAccountNumbersAsync(IEnumerable<string> accountNumbers);
        Task UpdateMoneyRequestAsync(MoneyRequest moneyRequest);

    }
}
