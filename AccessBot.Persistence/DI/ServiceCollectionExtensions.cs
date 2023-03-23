using AccessBot.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AccessBot.Persistence.DI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
    {
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddDbContext<AppDbContext, AppDbContextImpl>(configure);

        return services;
    }
}