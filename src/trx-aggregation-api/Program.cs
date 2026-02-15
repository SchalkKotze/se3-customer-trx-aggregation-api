using System.Diagnostics;
using aggregate_api.Application.Infrastructure.Categorisation;
using aggregate_api.Application.Infrastructure.Normalisation;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.Authentication;
using aggregate_api.Core.AutoMapper;
using aggregate_api.Core.Filters;
using aggregate_api.Core.FluentValidators;
using aggregate_api.Core.Serilog;
using aggregate_api.Core.Swagger;
using aggregate_api.Infrastructure;
using aggregate_api.Infrastructure.Contracts;
using aggregate_api.Infrastructure.ExternalData;


var builder = WebApplication.CreateBuilder(args);

// Register Authentication
builder.Services.UseCustomJwtBearer();
builder.Services.AddScoped<CorrelationIdFilter>();

// Register Serilog
builder.InjectSerilog();

// Register Controllers
builder.Services.AddControllers();

// Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.InjectSwaggerFilters();

// Register AutoMapper Profiles
builder.Services.InjectAutoMapperProfiles();

// Register Fluent Validators
builder.Services.InjectFluentValidators();

// Register your Sources
builder.Services.AddScoped<ITransactionSource, BankSource>();
builder.Services.AddScoped<ITransactionSource, CreditSource>();
//builder.Services.AddScoped<ITransactionSource, InvestmentSource>();

builder.Services.AddScoped<ITransactionNormaliser, TransactionNormaliser>();
builder.Services.AddScoped<ITransactionCategoriser, TransactionCategoriser>();

// Register Services
builder.Services
    .AddTransient<IAzureTokenService, AzureTokenService>()
    .AddTransient<IAggregateService, AggregateService>();

// Register Infrastructure Services
builder.Services
    .AddSingleton<IEnvironmentService, EnvironmentService>()
    .AddSingleton<ILoggingService, LoggingService>();

// Register Other
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

var killswitch = Environment.GetEnvironmentVariable("SWAGGER_KILLSWITCH");

if (!string.IsNullOrEmpty(killswitch) && !bool.Parse(killswitch))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Aggregation API V1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Customer Aggregation API V2");
        c.SwaggerEndpoint("/swagger/v3/swagger.json", "Customer Aggregation API V3");
        c.SwaggerEndpoint("/swagger/v4/swagger.json", "Customer Aggregation API V4");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();