using System;
using System.Threading.Tasks;
using AccountService.Domain.Entities;

namespace AccountService.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account> AddAsync(Account account);
        Task<Account?> GetByIdAsync(Guid accountId);
        Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId);
        Task<Account> UpdateAsync(Account account);
    }
}
