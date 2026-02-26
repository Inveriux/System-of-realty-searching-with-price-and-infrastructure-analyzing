using BasicBuildingBlocks.Endpoints.Abstractions;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace BasicBuildingBlocks.Endpoints.Extensions;

public static class EndpointsRegistrarExtension
{
    public static WebApplication MapEndpoints(this WebApplication application, Assembly assembly)
    {
        var endpointObjectType = typeof(IEndpointRegistrar);

        var endpointGroups = assembly
            .GetExportedTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                endpointObjectType.IsAssignableFrom(t)); 

        foreach(var endpointGroup in endpointGroups)
        {
            if(Activator.CreateInstance(endpointGroup) is IEndpointRegistrar instance)
            {
                instance.MapEndpoints(application);
            }
        }

        return application;
    }

    public static WebApplication MapEndpoints(this WebApplication application)
    {
        return MapEndpoints(application, Assembly.GetExecutingAssembly());
    }
}