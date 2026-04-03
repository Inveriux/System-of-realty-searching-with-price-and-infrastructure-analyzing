using System.Reflection;
using BasicBuildingBlocks.Behaviors;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SearchService.Data;
using SearchService.Data.Repositories;
using SearchService.Data.Repositories.Abstractions;
using SearchService.Services.InternalClients;
using SearchService.Services.InternalClients.Abstractions;
using SearchService.Services.Ranking;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"))
        .UseSnakeCaseNamingConvention());

builder.Services.AddOptions<ListingServiceClientOptions>()
    .BindConfiguration(ListingServiceClientOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "Listing service base url is required.")
    .ValidateOnStart();

builder.Services.AddOptions<InfrastructureServiceClientOptions>()
    .BindConfiguration(InfrastructureServiceClientOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "Infrastructure service base url is required.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IListingServiceClient, ListingServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ListingServiceClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddHttpClient<IInfrastructureServiceClient, InfrastructureServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<InfrastructureServiceClientOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
});

builder.Services.AddScoped<ISearchListingRepository, SearchListingRepository>();
builder.Services.AddScoped<ISearchProfileProvider, SearchProfileProvider>();
builder.Services.AddScoped<ISearchRankingService, SearchRankingService>();

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
    await dbContext.Database.ExecuteSqlRawAsync("""
        ALTER TABLE search_listing
            ADD COLUMN IF NOT EXISTS agent_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
            ADD COLUMN IF NOT EXISTS title character varying(300) NOT NULL DEFAULT '',
            ADD COLUMN IF NOT EXISTS description character varying(4000),
            ADD COLUMN IF NOT EXISTS listing_details_type character varying(100),
            ADD COLUMN IF NOT EXISTS price_amount_usd numeric(18,2),
            ADD COLUMN IF NOT EXISTS main_media_url character varying(1000),
            ADD COLUMN IF NOT EXISTS media_count integer NOT NULL DEFAULT 0,
            ADD COLUMN IF NOT EXISTS infrastructure_score numeric(10,4),
            ADD COLUMN IF NOT EXISTS education_score numeric(10,4),
            ADD COLUMN IF NOT EXISTS healthcare_score numeric(10,4),
            ADD COLUMN IF NOT EXISTS transport_score numeric(10,4),
            ADD COLUMN IF NOT EXISTS commerce_score numeric(10,4),
            ADD COLUMN IF NOT EXISTS leisure_score numeric(10,4),
            ADD COLUMN IF NOT EXISTS education_count integer,
            ADD COLUMN IF NOT EXISTS healthcare_count integer,
            ADD COLUMN IF NOT EXISTS transport_count integer,
            ADD COLUMN IF NOT EXISTS commerce_count integer,
            ADD COLUMN IF NOT EXISTS leisure_count integer,
            ADD COLUMN IF NOT EXISTS education_nearest_meters numeric(10,2),
            ADD COLUMN IF NOT EXISTS healthcare_nearest_meters numeric(10,2),
            ADD COLUMN IF NOT EXISTS transport_nearest_meters numeric(10,2),
            ADD COLUMN IF NOT EXISTS commerce_nearest_meters numeric(10,2),
            ADD COLUMN IF NOT EXISTS leisure_nearest_meters numeric(10,2),
            ADD COLUMN IF NOT EXISTS pharmacy_count_primary integer,
            ADD COLUMN IF NOT EXISTS store_count_primary integer,
            ADD COLUMN IF NOT EXISTS stops_count_primary integer,
            ADD COLUMN IF NOT EXISTS stop_groups_count_primary integer,
            ADD COLUMN IF NOT EXISTS stop_platforms_count_primary integer,
            ADD COLUMN IF NOT EXISTS parking_count_primary integer,
            ADD COLUMN IF NOT EXISTS school_count_primary integer,
            ADD COLUMN IF NOT EXISTS kindergarten_count_primary integer,
            ADD COLUMN IF NOT EXISTS clinic_count_primary integer,
            ADD COLUMN IF NOT EXISTS hospital_count_primary integer,
            ADD COLUMN IF NOT EXISTS nearest_pharmacy_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_store_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_stop_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_stop_group_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_stop_group_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_stop_group_drive_minutes_estimated boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS nearest_stop_group_name character varying(300),
            ADD COLUMN IF NOT EXISTS nearest_stop_group_platform_count integer,
            ADD COLUMN IF NOT EXISTS nearest_parking_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_parking_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_parking_drive_minutes_estimated boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS nearest_school_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_kindergarten_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_clinic_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_clinic_drive_minutes_estimated boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS nearest_hospital_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_hospital_drive_minutes_estimated boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS parks_count_primary integer,
            ADD COLUMN IF NOT EXISTS nearest_park_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS fitness_count_primary integer,
            ADD COLUMN IF NOT EXISTS nearest_fitness_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_fitness_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS museums_count_primary integer,
            ADD COLUMN IF NOT EXISTS nearest_museum_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS theaters_count_primary integer,
            ADD COLUMN IF NOT EXISTS nearest_theater_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS cinemas_count_primary integer,
            ADD COLUMN IF NOT EXISTS nearest_cinema_drive_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_metro_walk_minutes numeric(10,2),
            ADD COLUMN IF NOT EXISTS nearest_metro_name character varying(300),
            ADD COLUMN IF NOT EXISTS metro_found_in_primary_radius boolean NOT NULL DEFAULT false,
            ADD COLUMN IF NOT EXISTS search_features_json jsonb NOT NULL DEFAULT '{{}}'::jsonb,
            ADD COLUMN IF NOT EXISTS created_at_utc timestamp without time zone NOT NULL DEFAULT now(),
            ADD COLUMN IF NOT EXISTS updated_at_utc timestamp without time zone NOT NULL DEFAULT now();

        CREATE UNIQUE INDEX IF NOT EXISTS ix_search_listing_listing_id ON search_listing (listing_id);
        CREATE INDEX IF NOT EXISTS ix_search_listing_location_id ON search_listing (location_id);
        CREATE INDEX IF NOT EXISTS ix_search_listing_agent_id ON search_listing (agent_id);
        CREATE INDEX IF NOT EXISTS ix_search_listing_listing_type ON search_listing (listing_type);
        CREATE INDEX IF NOT EXISTS ix_search_listing_building_type ON search_listing (building_type);
        CREATE INDEX IF NOT EXISTS ix_search_listing_price_amount ON search_listing (price_amount);
        CREATE INDEX IF NOT EXISTS ix_search_listing_published_date ON search_listing (published_date);
        CREATE INDEX IF NOT EXISTS ix_search_listing_infrastructure_score ON search_listing (infrastructure_score);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_store_walk_minutes ON search_listing (nearest_store_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_pharmacy_walk_minutes ON search_listing (nearest_pharmacy_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_stop_walk_minutes ON search_listing (nearest_stop_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_parking_walk_minutes ON search_listing (nearest_parking_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_school_walk_minutes ON search_listing (nearest_school_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_kindergarten_walk_minutes ON search_listing (nearest_kindergarten_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_clinic_drive_minutes ON search_listing (nearest_clinic_drive_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_hospital_drive_minutes ON search_listing (nearest_hospital_drive_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_park_walk_minutes ON search_listing (nearest_park_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_fitness_walk_minutes ON search_listing (nearest_fitness_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_museum_drive_minutes ON search_listing (nearest_museum_drive_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_theater_drive_minutes ON search_listing (nearest_theater_drive_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_cinema_drive_minutes ON search_listing (nearest_cinema_drive_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_metro_walk_minutes ON search_listing (nearest_metro_walk_minutes);
        CREATE INDEX IF NOT EXISTS ix_search_listing_metro_found_in_primary_radius ON search_listing (metro_found_in_primary_radius);
        """);
}

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Search Service is working.");

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
