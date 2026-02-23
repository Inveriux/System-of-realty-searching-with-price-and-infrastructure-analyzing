using ListingService.Application.UseCases.BathroomTypes;
using ListingService.Infrastructure.Data;
using ListingService.Infrastructure;
using ListingService.Application;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();
builder.AddInfrastructure();

var app = builder.Build();

if(builder.Environment.IsDevelopment())
{
    await app.InitializeTestDatabaseAsync();
}

app.MapGet("/bathroom_types", async (ISender sender, 
    CancellationToken cancellationToken) => await sender.Send(new GetBathroomTypesQuery(), cancellationToken));

app.MapPost("/bathroom_types/{name}", async (ISender sender, string name) =>
{
    await sender.Send(new CreateBathroomTypeCommand(name));
});

app.Run();
