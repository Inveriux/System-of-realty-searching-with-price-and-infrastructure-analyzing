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

if(builder.Environment.IsDevelopment())
{
    System.Console.WriteLine("Using Dev CORS policy");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", policy =>
        {
            policy
                .WithOrigins("http://localhost:8082", "http://localhost:8081") // Next dev server
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // нужно, если будут cookie/credentials
        });
    });   
}

if(builder.Environment.IsProduction())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    });   
}

var app = builder.Build();

app.UseExceptionHandler();

if(builder.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    await app.InitializeTestDatabaseAsync(false);
}

if(builder.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    await app.InitializeTestDatabaseAsync(false);
}

app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Listning Service is work.");

app.Run();
