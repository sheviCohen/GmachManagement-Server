using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Interfaces;
using Gmach.Core.Settings;
using Gmach.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Gmach.Data.Seed
{
    // מחיל migrations חסרות ויוצר נתוני התחלה. בטוח להרצה חוזרת:
    // כל חלק נוצר רק אם הוא עדיין לא קיים.
    public class DbSeeder
    {
        private readonly GmachDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public DbSeeder(GmachDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task SeedAsync(
            SeedSettings settings,
            CancellationToken cancellationToken = default)
        {
            await _context.Database.MigrateAsync(cancellationToken);

            await SeedUsersAsync(settings, cancellationToken);
            await SeedLoansAsync(cancellationToken);
        }

        private async Task SeedUsersAsync(
            SeedSettings settings,
            CancellationToken cancellationToken)
        {
            if (!await _context.Users.AnyAsync(
                    u => u.Role == UserRole.Admin, cancellationToken))
            {
                RequireSetting(settings.AdminEmail, "Seed:AdminEmail");
                RequireSetting(settings.AdminPassword, "Seed:AdminPassword");

                _context.Users.Add(new User
                {
                    FirstName = "מנהל",
                    LastName = "הגמח",
                    Email = settings.AdminEmail,
                    Phone = "0500000000",
                    PasswordHash = _passwordHasher.Hash(settings.AdminPassword),
                    Role = UserRole.Admin
                });
            }

            if (!string.IsNullOrWhiteSpace(settings.UserEmail) &&
                !await _context.Users.AnyAsync(
                    u => u.Email == settings.UserEmail, cancellationToken))
            {
                RequireSetting(settings.UserPassword, "Seed:UserPassword");

                _context.Users.Add(new User
                {
                    FirstName = "לקוח",
                    LastName = "לדוגמה",
                    Email = settings.UserEmail,
                    Phone = "0500000001",
                    PasswordHash = _passwordHasher.Hash(settings.UserPassword),
                    Role = UserRole.User
                });
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task SeedLoansAsync(CancellationToken cancellationToken)
        {
            if (await _context.LoanCategories.AnyAsync(cancellationToken) ||
                await _context.Loans.AnyAsync(cancellationToken))
                return;

            var wedding = new LoanCategory { Name = "חתונה" };
            var education = new LoanCategory { Name = "לימודים" };
            var emergency = new LoanCategory { Name = "חירום" };
            var general = new LoanCategory { Name = "כללי" };

            var now = DateTime.UtcNow;

            _context.Loans.AddRange(
                new Loan { Amount = 500, DueDate = now.AddMonths(2), Categories = { general } },
                new Loan { Amount = 1000, DueDate = now.AddMonths(3), Categories = { emergency, general } },
                new Loan { Amount = 2000, DueDate = now.AddMonths(4), Categories = { education } },
                new Loan { Amount = 5000, DueDate = now.AddMonths(6), Categories = { wedding, general } },
                new Loan { Amount = 10000, DueDate = now.AddMonths(8), Categories = { wedding } });

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static void RequireSetting(string value, string key)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Missing configuration value '{key}'. " +
                    "Set it with user-secrets (development) or an environment variable.");
        }
    }
}
