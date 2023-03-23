using AccessBot.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace AccessBot.Application.Services;

public abstract class AppDbContext : DbContext, IUnitOfWork
{
    protected AppDbContext(DbContextOptions options) : base(options) {}

    public abstract DbSet<User> Users { get; }
}