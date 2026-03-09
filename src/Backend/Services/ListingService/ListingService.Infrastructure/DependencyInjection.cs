using ListingService.Application.Contracts.Repositories;
using ListingService.Infrastructure.Data.Repositories;
using ListingService.Application.Contracts.Cache;
using Microsoft.Extensions.DependencyInjection;
using ListingService.Infrastructure.Cache;
using Microsoft.Extensions.Configuration;
using ListingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ListingService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var confgiuration = builder.Configuration;

        services.AddDbContext<ApplicationDbContext>((sp, b) => 
            b.UseNpgsql(confgiuration.GetConnectionString("PostgreSqlConnection"))
                .UseSnakeCaseNamingConvention()
                .UseSeedingAsync(sp));

        services.AddScoped<ApplicationDatabaseInitializer>();

        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped(typeof(ILookupRepository<>), typeof(LookupRepository<>));

        services.AddMemoryCache();
        services.AddScoped<ILookupCache, LookupCache>();
    }
}