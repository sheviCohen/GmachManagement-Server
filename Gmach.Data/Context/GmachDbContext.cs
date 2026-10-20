using Gmach.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gmach.Data.Context
{
    public class GmachDbContext : DbContext
    {
        public GmachDbContext(DbContextOptions<GmachDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Loan> Loans { get; set; } = null!;
        public DbSet<LoanCategory> LoanCategories { get; set; } = null!;
        public DbSet<LoanRequest> LoanRequests { get; set; } = null!;
        public DbSet<Repayment> Repayments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- User ----------
            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                e.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.Phone).HasMaxLength(20).IsRequired();
                e.Property(u => u.PasswordHash).IsRequired();

                // אימייל ייחודי
                e.HasIndex(u => u.Email).IsUnique();

                // שומרים את ה-enum כטקסט כדי שיהיה קריא בבסיס הנתונים
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            });

            // ---------- Loan ----------
            modelBuilder.Entity<Loan>(e =>
            {
                e.Property(l => l.Amount).HasPrecision(18, 2);
                e.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);

                // Optimistic Concurrency: ה-Version נכנס לתנאי ה-WHERE של כל UPDATE
                e.Property(l => l.Version).IsConcurrencyToken();
            });

            // ---------- LoanCategory ----------
            modelBuilder.Entity<LoanCategory>(e =>
            {
                e.Property(c => c.Name).HasMaxLength(50).IsRequired();
                e.HasIndex(c => c.Name).IsUnique();
            });

            // Loan <-> LoanCategory: Many-to-Many (EF יוצר את טבלת הקשר LoanCategoryLinks)
            modelBuilder.Entity<Loan>()
                .HasMany(l => l.Categories)
                .WithMany(c => c.Loans)
                .UsingEntity(j => j.ToTable("LoanCategoryLinks"));

            // ---------- LoanRequest ----------
            modelBuilder.Entity<LoanRequest>(e =>
            {
                e.Property(r => r.RequestedAmount).HasPrecision(18, 2);
                e.Property(r => r.Note).HasMaxLength(500);
                e.Property(r => r.RejectionReason).HasMaxLength(500);
                e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            });

            // User -> LoanRequests: One-to-Many
            modelBuilder.Entity<LoanRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.LoanRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Loan -> LoanRequests: One-to-Many
            modelBuilder.Entity<LoanRequest>()
                .HasOne(r => r.Loan)
                .WithMany(l => l.Requests)
                .HasForeignKey(r => r.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---------- Repayment ----------
            // LoanRequest -> Repayment: One-to-One
            modelBuilder.Entity<Repayment>()
                .HasOne(p => p.LoanRequest)
                .WithOne(r => r.Repayment)
                .HasForeignKey<Repayment>(p => p.LoanRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // המנהל שרשם את ההחזר
            modelBuilder.Entity<Repayment>()
                .HasOne(p => p.RecordedByUser)
                .WithMany()
                .HasForeignKey(p => p.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        // מחליף את ה-Version בכל עדכון של הלוואה, כך שאף service לא צריך לזכור לעשות זאת.
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<Loan>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Version = Guid.NewGuid();
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
