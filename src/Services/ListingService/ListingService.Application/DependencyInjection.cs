using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace ListingService.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder hostApplicationBuilder)
    {
        var services = hostApplicationBuilder.Services;
        var confgiuration = hostApplicationBuilder.Configuration;

        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
    }
}