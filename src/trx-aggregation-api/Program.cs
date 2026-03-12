using System.Diagnostics;
using aggregate_api.Application.Infrastructure.Categorisation;
using aggregate_api.Application.Infrastructure.Normalisation;
using aggregate_api.Application.Interfaces;
using aggregate_api.Application.Services;
using aggregate_api.Application.Services.Contracts;
using aggregate_api.Core.Authentication;
using aggregate_api.Core.AutoMapper;
using aggregate_api.Core.FluentValidators;
using aggregate_api.Core.ProblemDetails;
using aggregate_api.Core.Serilog;
using aggregate_api.Core.Swagger;
using aggregate_api.Infrastructure;
using aggregate_api.Infrastructure.Contracts;
using aggregate_api.Infrastructure.ExternalData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StackExchange.Redis;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------
//  Authentication
// ----------------------------------------
var useLocalFakeJwt = Environment.GetEnvironmentVariable(
    "USE_LOCAL_FAKE_JWT") == "true";

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
//  Serilog
// ----------------------------------------
builder.InjectSerilog();

// Get PostgreSQL connection string from environment variables
var postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRE_CONNECTION_STRING")
                               ?? throw new InvalidOperationException("PostgreSQL connection string is not set in environment variables.");

// Initialize the database and insert test data
InitializeDatabase(postgresConnectionString);
// Redis
// Add Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
    if (string.IsNullOrEmpty(redisConnectionString))
    {
        throw new InvalidOperationException("Redis connection string is not set in environment variables.");
    }
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

// ----------------------------------------
//  Controllers
// ----------------------------------------
builder.Services.AddControllers()
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Uniform model validation responses - RFC 7807 Problem Details
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => e.Key,
                e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray()
            );

        var problem = ProblemDetailsFactory.CreateValidationErrorsProblem(
            errors: errors,
            instance: context.HttpContext.Request.Path
        );

        return new Microsoft.AspNetCore.Mvc.UnprocessableEntityObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// ----------------------------------------
//  Swagger
// ----------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.InjectSwaggerFilters();


// ----------------------------------------
//  AutoMapper & Fluent Validators
// ----------------------------------------
builder.Services.InjectAutoMapperProfiles();
builder.Services.InjectFluentValidators();

// ----------------------------------------
//  Transaction Sources & Services
// ----------------------------------------
builder.Services.AddScoped<ITransactionSource, BankSourceDatabase>();
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
//  Middleware
// ----------------------------------------
app.UseProblemDetailsExceptionHandling();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

var killswitch = Environment.GetEnvironmentVariable("SWAGGER_KILLSWITCH");

if (!string.IsNullOrEmpty(killswitch) && !bool.Parse(killswitch))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customer Aggregation API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

void InitializeDatabase(string connectionString)
{
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    // Create the table if it doesn't exist
    using (var command = connection.CreateCommand())
    {
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS BankTransactions (
                BankTransactionID SERIAL PRIMARY KEY,
                CustomerID VARCHAR(50) NOT NULL,
                Source VARCHAR(100),
                Amount DECIMAL NOT NULL,
                Description TEXT,
                TransactiopnDate TIMESTAMP NOT NULL
            );
        ";
        command.ExecuteNonQuery();
    }

    // Insert test data
    using (var command = connection.CreateCommand())
    {
        command.CommandText = @"
            TRUNCATE TABLE BankTransactions RESTART IDENTITY; -- Clear existing data and reset ID sequence
            
            INSERT INTO BankTransactions (CustomerID, Source, Amount, Description, TransactiopnDate)
            VALUES
                ('1', 'BX', 100.50, 'Spar', NOW()),
                ('2', 'BX', 200.75, 'Foodlovers', NOW())
               
            ON CONFLICT DO NOTHING;
        ";
        command.ExecuteNonQuery();
    }
}

