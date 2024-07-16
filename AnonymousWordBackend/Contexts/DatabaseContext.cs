using AnonymousWordBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AnonymousWordBackend.Contexts;

public sealed class DatabaseContext : DbContext
{
    public DbSet<UserModel> Users { get; set; }
    public DbSet<MessageModel> Messages { get; set; }
    public DbSet<BanListModel> BanList { get; set; }

    public DatabaseContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=anonymous-word;Username=anonymous-word;Password=anonymous-word");
    }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserModel>()
            .HasIndex(user => user.TelegramId)
            .IsUnique();
        
        modelBuilder.Entity<UserModel>()
            .Property(user => user.Roles)
            .HasConversion<short>();

        modelBuilder.Entity<UserModel>()
            .HasMany(user => user.BanList)
            .WithOne(banList => banList.Issuer)
            .HasForeignKey(banList => banList.IssuerId);

        modelBuilder.Entity<MessageModel>()
            .HasOne(message => message.Recipient)
            .WithMany(user => user.ReceivedMessages)
            .HasForeignKey(message => message.RecipientId);

        modelBuilder.Entity<MessageModel>()
            .HasOne(message => message.Author)
            .WithMany(user => user.SentMessages)
            .HasForeignKey(message => message.AuthorId);
        
        modelBuilder.Entity<BanListModel>()
            .HasOne(banList => banList.Issuer)
            .WithMany()
            .HasForeignKey(banList => banList.IssuerId);
        
        modelBuilder.Entity<BanListModel>()
            .HasOne(banList => banList.Banned)
            .WithMany()
            .HasForeignKey(banList => banList.BannedId);
        
        base.OnModelCreating(modelBuilder);
    }
}