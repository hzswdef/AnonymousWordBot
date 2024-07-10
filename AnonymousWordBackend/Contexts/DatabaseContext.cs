using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Contexts;

public sealed class DatabaseContext : DbContext
{
    public DbSet<UserModel> Users { get; set; }
    public DbSet<MessageModel> Messages { get; set; }

    public DatabaseContext()
    {
        Database.EnsureCreated();
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Database=anonymous-word;Username=anonymous-word;Password=anonymous-word");
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .HasIndex(user => user.TelegramId)
            .IsUnique();
        
        modelBuilder.Entity<UserModel>()
            .Property(user => user.Roles)
            .HasConversion<short>();
        
        base.OnModelCreating(modelBuilder);
    }
}