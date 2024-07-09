using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Contexts;

public sealed class DatabaseContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DatabaseContext()
    {
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Database=anonymous-word;Username=anonymous-word;Password=anonymous-word");
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.TelegramId)
            .IsUnique();
        
        base.OnModelCreating(modelBuilder);
    }
}