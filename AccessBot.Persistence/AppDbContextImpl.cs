using AccessBot.Application.Models;
using AccessBot.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace AccessBot.Persistence;

public sealed class AppDbContextImpl : AppDbContext
{
    public AppDbContextImpl(DbContextOptions options) : base(options)
    {
    }

    public override DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.ToTable(nameof(User));
        user.Property(x => x.Balance).IsRequired();
        user.HasKey(x => x.Id);
        user.HasOne(x => x.Inviter)
            .WithMany(x => x.Referrals);
    }
}