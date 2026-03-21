using System.Reflection;
using BasicBuildingBlocks.Behaviors;
using BasicBuildingBlocks.Endpoints.Extensions;
using BasicBuildingBlocks.Handlers;
using FluentValidation;
using ListingPageBff.Application.ListingPages;
using ListingPageBff.Infrastructure.Clients;
using ListingPageBff.Infrastructure.Clients.Abstractions;
using ListingPageBff.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ListingPageAssembler>();

builder.Services.AddOptions<ListingServiceOptions>()
    .BindConfiguration(ListingServiceOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "ListingService base url is required.")
    .ValidateOnStart();

builder.Services.AddOptions<InfrastructureServiceOptions>()
    .BindConfiguration(InfrastructureServiceOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "InfrastructureService base url is required.")
    .ValidateOnStart();

builder.Services.AddOptions<AgentServiceOptions>()
    .BindConfiguration(AgentServiceOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "AgentService base url is required.")
    .ValidateOnStart();

builder.Services.AddOptions<SearchServiceOptions>()
    .BindConfiguration(SearchServiceOptions.SectionName)
    .Validate(x => !string.IsNullOrWhiteSpace(x.BaseUrl), "SearchService base url is required.")
    .ValidateOnStart();

builder.Services.AddHttpClient<IListingServiceClient, ListingServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ListingServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddHttpClient<IInfrastructureServiceClient, InfrastructureServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<InfrastructureServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddHttpClient<IAgentServiceClient, AgentServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<AgentServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddHttpClient<ISearchServiceClient, SearchServiceClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<SearchServiceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

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
app.MapGet("/", () => "Listing page BFF is working.");

app.Run();
