# se3-customer-trx-aggregation-api
Api to aggregate customer financial transactions

This branch contains ongoing development for the Transaction Aggregation API

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

## How to Build and Run 
 
To Build the Docker Container : docker compose build --no-cache from the src directory
To Run ,from the src directory : docker run --env-file .env -p 5000:8080 src-aggregation-api
Then in the Browser : http://localhost:5000/swagger/index.html

## How to Test

cd trx-aggreagtion-api.Test
dotnet test

3 Positive Tests
1 Negative Test

Test Data

Raw transactions are mocked from a DBsource (BX) Banks, a Kafka Source (CX) Credit , and an Api (Investments) Investments

Endpoints 

    /api/v1/aggregation/categories
    Sample Request Object

    {
    "customerIds": [
    "1","2"
    ],
    "fromDate": "2025-02-20T14:09:51.267Z",
    "toDate": "2026-02-20T14:09:51.267Z",
    "sourceSystem": "CX"
    }


Test Data only exist for Customer 1 and 2.
Sources are BX (banks),CX (Credit)




## JWT
All endpoints are [Authorize]

I have by means of an Environment Var in the .env supplied a toggle [USE_LOCAL_FAKE_JWT] that will
    TRUE : Fake a Token for ease of use for Evaluator
    FALSE : JWT token needs to be aquired by using the Token Endpoint that will supply a Bearer token that needs to be copied into the Swagger Autherize field which will then add the bearer into the Header of all Endpoint Calls

            CLientID : a0dccd5c-8550-4fc3-a326-1336641f3ca0
            Secret : Will be supplied if required.

## Swagger Implemented
    I have by means of an Environment Var in the .env supplied a toggle [SWAGGER_KILLSWITCH] 
    TRUE : Will not expose Swagger
    FALSE: Expose Swagger


## Automapper is Implemented
    To Map Requests to Standerised command

## FluentValidator is Implemented
    To do validation of commands

## Serilog is Implemented 
    SeriLog implemented as logging provider that replaces the default ILogger 
    Obviously cannot reach the AWS Cloudwatch (Env Var was created to AWS Loggroup)
    So will try to log to aws-logger-errors.txt

## Health Checks Implemented
    /health/live
    /health/ready

## Transaction Aggregation Service Overview

The Transaction Aggregation Service provides a unified way to retrieve and analyze customer transactions across multiple sources. At its core, the service implements a reusable pipeline that ensures consistency, maintainability, and extensibility for all aggregation endpoints. The pipeline follows a clear sequence: Get → Normalise → Filter → Categorise.

Get – The service collects raw transactions from multiple configured sources (e.g., bank systems, credit systems, Kafka topics). Each source is queried independently, allowing partial failures to be logged without breaking the pipeline.

Normalise – All raw transactions are converted into a standard, consistent Transaction model. This ensures that downstream processing is source-agnostic, regardless of differences in naming conventions, formats, or fields across sources.

Filter – Transactions are filtered based on user-supplied criteria, such as customer ID, date range, or source system. This allows precise, on-demand queries while keeping the pipeline generic.

Categorise – Finally, transactions are categorised using a dedicated TransactionCategoriser service. Categories are then used to compute balances, spend summaries, and monthly reports in a consistent, reusable way.

This pipeline is leveraged by all service endpoints — including Aggregate Transactions, Balances, Spend by Category, and Monthly Summary — ensuring that any new aggregation functionality can reuse the same reliable flow without duplicating code.    

## Extensibility

The service is highly extensible. To integrate a new transaction source, developers simply implement the ITransactionSource interface and register the source with dependency injection. The pipeline automatically incorporates the new source into all aggregation endpoints without requiring changes to validation, filtering, or categorisation logic. This design ensures that as new systems or data streams are added, the service scales without introducing duplication or complexity.


## Solution Structure Tree
.\
├── README.md\
├── project.structure.txt\
├── trx-aggregation-api\
│   ├── API\
│   │   ├── Controllers\
│   │   │   └── v1\
│   │   │       ├── AggregateController.cs\
│   │   │       └── TokensController.cs\
│   │   └── DTOs\
│   │       ├── Requests\
│   │       │   └── v1\
│   │       │       ├── AggregateRequest.cs\
│   │       │       └── GenerateTokenRequest.cs\
│   │       └── Responses\
│   │           └── v1\
│   │               ├── AggregateResponse.cs\
│   │               └── TokenResponse.cs\
│   ├── Application\
│   │   ├── Commands\
│   │   │   └── CustomerAggregationCommand.cs\
│   │   ├── Domain\
│   │   │   ├── Constants\
│   │   │   │   ├── CategoryLookups.cs\
│   │   │   │   ├── ContentTypeLookups.cs\
│   │   │   │   ├── ErrorMessages.cs\
│   │   │   │   ├── LoggingMessages.cs\
│   │   │   │   └── ValidationMessages.cs\
│   │   │   ├── Enums\
│   │   │   │   ├── ClientTypeEnum.cs\
│   │   │   │   ├── LogLevelEnum.cs\
│   │   │   │   └── TransactionCategory.cs\
│   │   │   └── Models\
│   │   │       ├── ResponseModel.cs\
│   │   │       ├── Transaction.cs\
│   │   │       └── TransactionCategory.cs\
│   │   ├── Dtos\
│   │   │   ├── AggregatedCategoryResultsDtos.cs\
│   │   │   ├── AggregatedCustomerTransactionsDto.cs\
│   │   │   └── GenerateTokenDto.cs\
│   │   ├── Interfaces\
│   │   │   ├── IAggregateService.cs\
│   │   │   ├── IAzureTokenService.cs\
│   │   │   ├── ITransactionCategoriser.cs\
│   │   │   └── ITransactionNormaliser.cs\
│   │   └── Services\
│   │       ├── AggregateService.cs\
│   │       └── AzureTokenService.cs\
│   ├── Core\
│   │   ├── Authentication\
│   │   │   ├── AuthenticationBase.cs\
│   │   │   └── FakeJWTHandler.cs\
│   │   ├── AutoMapper\
│   │   │   ├── AutoMapperBase.cs\
│   │   │   ├── Extentions\
│   │   │   │   └── MessageDtoExtention.cs\
│   │   │   └── Profiles\
│   │   │       ├── AggregateProfileV1.cs\
│   │   │       └── TokenProfile.cs\
│   │   ├── FluentValidators\
│   │   │   ├── AggregateValidators\
│   │   │   │   └── AggregateCommandValidator.cs\
│   │   │   ├── FluentValidatorBase.cs\
│   │   │   ├── Services\
│   │   │   │   ├── Contracts\
│   │   │   │   │   └── IFluentValidationService.cs\
│   │   │   │   └── FluentValidationService.cs\
│   │   │   └── TokenValidators\
│   │   │       └── GenerateTokenValidator.cs\
│   │   ├── Serilog\
│   │   │   └── SerilogBase.cs\
│   │   └── Swagger\
│   │       ├── Filters\
│   │       │   └── SwaggerResponseFilter.cs\
│   │       ├── Groups\
│   │       │   ├── AggregationResponseGroup.cs\
│   │       │   └── TokenResponseGroup.cs\
│   │       ├── Helpers\
│   │       │   └── OpenApiHelper.cs\
│   │       └── SwaggerBase.cs\
│   ├── Infrastructure\
│   │   ├── Categorisation\
│   │   │   └── TransactionCategoriser.cs\
│   │   ├── Contracts\
│   │   │   ├── IEnvironmentService.cs\
│   │   │   └── ILoggingService.cs\
│   │   ├── EnvironmentService.cs\
│   │   ├── ExternalData\
│   │   │   ├── Fakes\
│   │   │   │   ├── BankSource.cs\
│   │   │   │   ├── CreditSource.cs\
│   │   │   │   └── InvestmentSource.cs\
│   │   │   ├── ITransactionSource.cs\
│   │   │   └── Production\
│   │   │       ├── BankSourceDatabase.cs\
│   │   │       ├── CreditSourceApi.cs\
│   │   │       └── InvestmentSourceKafka.cs\
│   │   ├── LoggingService.cs\
│   │   ├── Normailsation\
│   │   │   └── TransactionNormaliser.cs\
│   │   └── RawTransactions\
│   │       ├── BankSourceRawTransaction.cs\
│   │       ├── CreditRawTransactions.cs\
│   │       ├── InvestmentRawTransactions.cs\
│   │       └── RawTransaction.cs\
│   ├── Program.cs\
│   ├── Properties\
│   │   └── launchSettings.json\
│   ├── appsettings.json\
│   ├── aws-logger-errors.txt\
│   └── trx-aggregation-api.csproj\
├── trx-aggregation-api.Test\
│   ├── AggregateServiceTests.cs\
│   ├── CategoriseServiceTests.cs\
│   ├── NegativeServiceTests.cs\
│   └── trx-aggregation-api.Test.csproj\
├── trx-aggregation-api.sln\

## Architecture & Design Patterns

This service is designed as a modular, extensible transaction aggregation system, built around a reusable processing pipeline and well-defined abstraction boundaries. The focus is on scalability, resilience, and ease of extension when integrating additional data sources.

Core Aggregation Pipeline

At the heart of the system is a reusable processing pipeline that follows the flow:

Get → Normalise → Filter → Categorise → Aggregate

This pipeline is implemented once and reused across all aggregation endpoints (balances, spend-by-category, monthly summaries, and full transaction aggregation). By centralising this logic, the service guarantees consistent behaviour, validation, and categorisation regardless of the type of aggregation being performed.

This design closely resembles a Chain of Responsibility / Pipeline pattern, where each step has a single responsibility and feeds into the next stage.

## Strategy Pattern — Transaction Sources

External transaction providers are integrated using the Strategy pattern via the ITransactionSource interface.

Each transaction source:

Implements a common contract

Encapsulates its own retrieval logic

Can be added or removed without modifying existing service code

New data sources can be introduced simply by registering a new implementation of ITransactionSource, making the system open for extension but closed for modification.

## Adapter Pattern — External Integrations

Each transaction source also functions as an Adapter, translating external system-specific data formats into a common internal RawTransaction model. This isolates external variability and ensures that downstream logic operates on a consistent data structure.

## Facade Pattern — AggregateService

AggregateService acts as a Facade over the underlying system complexity. Controllers interact with a single application service, while the service itself coordinates:

Data source retrieval

Normalisation

Filtering

Categorisation

Aggregation

Validation and error handling

This keeps controllers thin and ensures that orchestration logic remains in one place.

Parallelised, Resilient Data Retrieval

Transaction sources are queried in parallel using asynchronous execution, allowing the system to scale efficiently as new sources are added. Failures in individual sources are isolated and logged, enabling partial success rather than failing the entire aggregation request.

This approach reflects real-world aggregation system behaviour, where upstream dependencies may be unreliable.

Extensibility & Maintainability

The system is intentionally designed for extensibility:

Adding a new data source requires no changes to existing aggregation logic

New aggregation endpoints reuse the same pipeline

Filtering, categorisation, and validation rules are applied consistently

This makes the service easy to evolve while maintaining predictable behaviour and a clean separation of concerns.