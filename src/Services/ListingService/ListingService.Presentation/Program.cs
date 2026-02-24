using BasicBuildingBlocks.Endpoints.Extensions;
using ListingService.Infrastructure.Data;
using ListingService.Infrastructure;
using BasicBuildingBlocks.Handlers;
using ListingService.Application;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddInfrastructure();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if(builder.Environment.IsDevelopment())
{
    await app.InitializeTestDatabaseAsync();
}

app.RegisterEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Listning Service is work.");

app.Run();
