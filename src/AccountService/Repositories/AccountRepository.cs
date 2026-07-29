using AccountService.Domain.Entities;
using AccountService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using AccountService.Data;

namespace AccountService.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AccountDbContext _dbContext;

        public AccountRepository(AccountDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Account> AddAsync(Account account)
        {
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
            return account;
        }
    }
}