using System.Reflection;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PricingAnalyticsService.Data;
using PricingAnalyticsService.Repositories;
using PricingAnalyticsService.Repositories.Abstractions;
using PricingAnalyticsService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddOptions<ListingServiceClientOptions>()
    .BindConfiguration(ListingServiceClientOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "Listing service base url is required.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IListingPriceHistoryClient, ListingPriceHistoryClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ListingServiceClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddScoped<IPricingAnalyticsRepository, PricingAnalyticsRepository>();

builder.Services.AddExceptionHandler<ExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("DefaultCors");
app.UseExceptionHandler();
app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Pricing Analytics Service is working.");

app.Run();
