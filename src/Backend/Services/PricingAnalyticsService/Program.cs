using System.Reflection;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await EnsureSearchListingSchemaAsync(dbContext);
}

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Pricing Analytics Service is working.");

app.Run();

static async Task EnsureSearchListingSchemaAsync(ApplicationDbContext dbContext)
{
    await dbContext.Database.EnsureCreatedAsync();

    var shouldCloseConnection = dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open;
    if (shouldCloseConnection)
    {
        await dbContext.Database.OpenConnectionAsync();
    }

    await using var command = dbContext.Database.GetDbConnection().CreateCommand();
    command.CommandText = """
        SELECT EXISTS (
            SELECT 1
            FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = 'search_listing')
        """;

    var existsResult = await command.ExecuteScalarAsync();
    var tableExists = existsResult is bool value && value;

    if (tableExists)
    {
        if (shouldCloseConnection)
        {
            await dbContext.Database.CloseConnectionAsync();
        }

        return;
    }

    var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();
    await databaseCreator.CreateTablesAsync();

    if (shouldCloseConnection)
    {
        await dbContext.Database.CloseConnectionAsync();
    }
}
