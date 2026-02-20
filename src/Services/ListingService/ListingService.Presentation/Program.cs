using ListingService.Application.UseCases.BathroomType;
using ListingService.Infrastructure.Data;
using ListingService.Infrastructure;
using ListingService.Application;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddInfrastructure();

System.Console.WriteLine(builder.Environment.EnvironmentName);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.MapGet("/bathroom_types", (ISender sender, 
    CancellationToken cancellationToken) => sender.Send(new GetBathroomTypesQuery(), cancellationToken));

app.Run();
