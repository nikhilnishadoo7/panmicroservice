using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using pan.Middleware;
using PAN.API.Application.Services.Implementations;
using PAN.API.Application.Services.Interfaces;
using PAN.API.Configurations;
using PAN.API.Infrastructure.Dapper;
using PAN.API.Infrastructure.Providers.Implementations;
using PAN.API.Infrastructure.Providers.Interfaces;
using PAN.API.Infrastructure.Repositories.Implementations;
using PAN.API.Infrastructure.Repositories.Interfaces;
using PAN.API.Infrastructure.Resilience;
using PAN.API.Middleware;
using PAN.API.Utilities;
using Serilog;

LoggerConfig.ConfigureLogger();

var builder = WebApplication.CreateBuilder(args);


Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;


builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Host.UseSerilog();

builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PAN API",
        Version = "v1"
    });
});


builder.Services.AddHttpClient("SurepassClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(PollyPolicies.GetRetryPolicy("SurePass"))
.AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy("SurePass"));

builder.Services.AddHttpClient("SprintVerifyClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(PollyPolicies.GetRetryPolicy("SprintVerify"))
.AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy("SprintVerify"));

builder.Services.AddScoped<ISurePassService, SurePassProvider>();
builder.Services.AddScoped<ISprintVerifyService, SprintVerifyProvider>();


builder.Services.AddSingleton<DapperContext>();


builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddHostedService<CacheWarmupService>();


builder.Services.AddScoped<IPanRepository, PanRepository>();
builder.Services.AddScoped<IRawResponseRepository, RawResponseRepository>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IHealthRepository, HealthRepository>();
builder.Services.AddScoped<IHealthService, HealthService>();


builder.Services.AddScoped<ISurePassService, SurePassProvider>();
builder.Services.AddScoped<ISprintVerifyService, SprintVerifyProvider>();

// Services
builder.Services.AddScoped<IFallbackService, ProviderFallbackService>();
builder.Services.AddScoped<IPanVerificationService, PanVerificationService>();

var app = builder.Build();
//app.UseMiddleware<GatewayAuthMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PAN API v1");
});

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }