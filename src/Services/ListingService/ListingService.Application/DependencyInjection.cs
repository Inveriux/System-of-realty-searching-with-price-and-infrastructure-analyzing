using ListingService.Application.Common.Lookups.Abstractions;
using ListingService.Application.Common.Lookups.Converters;
using ListingService.Application.Common.Lookups;
using Microsoft.Extensions.DependencyInjection;
using BasicBuildingBlocks.Behaviors;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using FluentValidation;
using MediatR;

namespace ListingService.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder hostApplicationBuilder)
    {
        var services = hostApplicationBuilder.Services;
        var confgiuration = hostApplicationBuilder.Configuration;

        services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
        services.AddTransient(typeof(LookupByNameConverter<>));
        services.AddTransient(typeof(LookupByNameOrNullConverter<>));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        services.AddScoped(typeof(ILookupHandler<,>), typeof(LookupHandler<,>));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}