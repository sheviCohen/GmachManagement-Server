using Gmach.Core.Entities;
using Gmach.Core.Enums;
using Gmach.Core.Exceptions;
using Gmach.Data.Context;
using Gmach.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gmach.Tests
{
    // בדיקת Optimistic Concurrency אמיתית: שני DbContext נפרדים על אותו בסיס נתונים.
    // משתמשים ב-SQLite בזיכרון, כי בניגוד ל-InMemory של EF הוא באמת בודק את ה-concurrency token.
    public class ConcurrencyTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<GmachDbContext> _options;

        public ConcurrencyTests()
        {
            // החיבור נשאר פתוח, אחרת בסיס הנתונים בזיכרון נמחק
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<GmachDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new GmachDbContext(_options);
            context.Database.EnsureCreated();
            context.Loans.Add(new Loan
            {
                Amount = 1000,
                DueDate = DateTime.UtcNow.AddMonths(1)
            });
            context.SaveChanges();
        }

        public void Dispose() => _connection.Dispose();

        private GmachDbContext NewContext() => new(_options);

        [Fact]
        public async Task TwoContextsReserveSameLoan_SecondSaveThrowsDbUpdateConcurrencyException()
        {
            using var first = NewContext();
            using var second = NewContext();

            // שני הלקוחות טוענים את אותה הלוואה, ושניהם רואים אותה זמינה
            var loanInFirst = await first.Loans.FirstAsync();
            var loanInSecond = await second.Loans.FirstAsync();

            loanInFirst.Reserve();
            await first.SaveChangesAsync();        // הראשון מצליח

            loanInSecond.Reserve();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                () => second.SaveChangesAsync());  // השני נכשל
        }

        [Fact]
        public async Task Repository_TranslatesConcurrencyFailureToConflictException()
        {
            using var first = NewContext();
            using var second = NewContext();
            var repositoryOfFirst = new LoanRepository(first);
            var repositoryOfSecond = new LoanRepository(second);

            var loanInFirst = await repositoryOfFirst.GetByIdAsync(1);
            var loanInSecond = await repositoryOfSecond.GetByIdAsync(1);

            loanInFirst!.Reserve();
            await repositoryOfFirst.SaveChangesAsync();

            loanInSecond!.Reserve();
            await Assert.ThrowsAsync<ConflictException>(
                () => repositoryOfSecond.SaveChangesAsync());
        }

        [Fact]
        public async Task SuccessfulUpdate_ChangesVersion()
        {
            using var context = NewContext();
            var loan = await context.Loans.FirstAsync();
            var versionBefore = loan.Version;

            loan.Reserve();
            await context.SaveChangesAsync();

            Assert.NotEqual(versionBefore, loan.Version);
            Assert.Equal(LoanStatus.Reserved, loan.Status);
        }
    }
}
