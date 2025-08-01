using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

public class BackendContext : DbContext
{
    public BackendContext(DbContextOptions<BackendContext> options) : base(options)
    {
    }

    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<QuizResult> QuizResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Questions)
            .WithOne(q => q.Quiz)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Answers)
            .WithOne(a => a.Question)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Results)
            .WithOne(r => r.Quiz)
            .HasForeignKey(r => r.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}