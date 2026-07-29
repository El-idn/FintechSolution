using AccountService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AccountService.Enums;

namespace AccountService.Data
{
    public class AccountDbContext : DbContext
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var accountTypeConverter = new EnumToStringConverter<AccountType>();
            modelBuilder.Entity<Account>()
                .Property(a => a.AccountType)
                .HasConversion(accountTypeConverter);
        }
    }
}
