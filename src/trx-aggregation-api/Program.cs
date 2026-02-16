using System.Diagnostics;
using aggregate_api.Application.Domain.Models;
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------
// 1️⃣ Authentication
// ----------------------------------------
var useLocalFakeJwt = Environment.GetEnvironmentVariable("USE_LOCAL_FAKE_JWT") == "true";

if (useLocalFakeJwt)
{
    // Fake JWT handler for evaluators
    builder.Services.AddAuthentication("FakeJwt")
        .AddScheme<AuthenticationSchemeOptions, FakeJwtHandler>("FakeJwt", _ => { });
}
else
{
    // Real Azure JWT
    builder.Services.UseCustomJwtBearer();
}

builder.Services.AddAuthorization(); // Required for [Authorize]

// ----------------------------------------
// 2️⃣ Filters & Serilog
// ----------------------------------------
builder.Services.AddScoped<CorrelationIdFilter>();
builder.InjectSerilog();

// ----------------------------------------
// 3️⃣ Controllers
// ----------------------------------------
builder.Services.AddControllers();

// Uniform model validation responses
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var response = new ResponseModel();
        foreach (var entry in context.ModelState)
        {
            foreach (var errors in entry.Value.Errors)
            {
                response.Errors.Add(
                    string.IsNullOrWhiteSpace(entry.Key)
                        ? errors.ErrorMessage
                        : $"{entry.Key}: {errors.ErrorMessage}"
                );
            }
        }
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
    };
});

// ----------------------------------------
// 4️⃣ Swagger
// ----------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.InjectSwaggerFilters();

// ----------------------------------------
// 5️⃣ AutoMapper & Fluent Validators
// ----------------------------------------
builder.Services.InjectAutoMapperProfiles();
builder.Services.InjectFluentValidators();

// ----------------------------------------
// 6️⃣ Transaction Sources & Services
// ----------------------------------------
builder.Services.AddScoped<ITransactionSource, BankSource>();
builder.Services.AddScoped<ITransactionSource, CreditSource>();
builder.Services.AddScoped<ITransactionNormaliser, TransactionNormaliser>();
builder.Services.AddScoped<ITransactionCategoriser, TransactionCategoriser>();

builder.Services
    .AddTransient<IAzureTokenService, AzureTokenService>()
    .AddTransient<IAggregateService, AggregateService>();

builder.Services
    .AddSingleton<IEnvironmentService, EnvironmentService>()
    .AddSingleton<ILoggingService, LoggingService>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// ----------------------------------------
// 7️⃣ Middleware
// ----------------------------------------
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

