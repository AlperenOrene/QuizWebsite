using Microsoft.EntityFrameworkCore;
using SoruDeneme.Models;

namespace SoruDeneme.Data
{
    public class SoruDenemeContext : DbContext
    {
        public SoruDenemeContext(DbContextOptions<SoruDenemeContext> options)
            : base(options)
        { }

        public DbSet<Question> Question { get; set; } = default!;
        public DbSet<Quiz> Quiz { get; set; } = default!;
        public DbSet<AppUser> Users { get; set; } = default!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = default!;
        public DbSet<AttemptAnswer> AttemptAnswers { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(z => z.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.OwnerTeacher)
                .WithMany()
                .HasForeignKey(q => q.OwnerTeacherId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
