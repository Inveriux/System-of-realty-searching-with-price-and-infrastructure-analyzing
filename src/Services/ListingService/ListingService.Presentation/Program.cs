using ListingService.Presentation.Converters.Json;
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

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new CreateListingDtoJsonConverter());
    o.SerializerOptions.Converters.Add(new ListingDetailsDtoWriteConverter());
});

var app = builder.Build();

//app.UseExceptionHandler();

if(builder.Environment.IsDevelopment())
{
    await app.InitializeTestDatabaseAsync();
}

app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Listning Service is work.");

app.Run();
