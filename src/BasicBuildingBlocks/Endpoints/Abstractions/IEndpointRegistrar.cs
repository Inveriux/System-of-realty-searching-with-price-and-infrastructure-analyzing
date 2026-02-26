using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;

namespace BasicBuildingBlocks.Endpoints.Abstractions;

public interface IEndpointRegistrar
{
    RouteGroupBuilder MapEndpoints(WebApplication application);
}
