using System.Reflection;
using BasicBuildingBlocks.Behaviors;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SearchService.Data;
using SearchService.Data.Repositories;
using SearchService.Data.Repositories.Abstractions;
using SearchService.Services.InternalClients;
using SearchService.Services.InternalClients.Abstractions;

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
    await dbContext.Database.EnsureCreatedAsync();
}

app.MapOpenApi();
app.MapEndpoints(Assembly.GetExecutingAssembly());
app.MapGet("/", () => "Search Service is working.");

app.Run();
