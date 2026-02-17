# se3-customer-trx-aggregation-api
Api to aggregate customer financial transactions

This branch contains ongoing development for the Transaction Aggregation API

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

## How to Build and Run 
 
To Build the Docker Container  : docker compose build --no-cache from the src directory
To Run ,from the src directory : docker run --env-file .env -p 5000:8080 src-aggregation-api
Then in the Browser : http://localhost:5000/swagger/index.html

## How to Test

cd trx-aggreagtion-api.Test
dotnet test

3 Positive Tests
1 Negative Test

## Autherization JWT is implelemted
All endpoints are [Authorize]

I have by means of an Environment Var in the .env supplied a toggle [USE_LOCAL_FAKE_JWT] that will
    TRUE : Fake a Token for ease of use for Evaluator
    FALSE : JWT token needs to be aquired by using the Token Endpoint that will supply a Bearer token that needs to be copied into the Swagger Autherize field which will then add the bearer into the Header of all Endpoint Calls

            CLientID : a0dccd5c-8550-4fc3-a326-1336641f3ca0
            Secret : Will be supplied if required.

## Swagger is Implemented
    I have by means of an Environment Var in the .env supplied a toggle [SWAGGER_KILLSWITCH] 
    TRUE : Will not expose Swagger
    FALSE: Expose Swagger

## Automapper is Implemented
    To Map Requests to DTOs

## FluentValidator is Implemented
    To do validation of commands

## Serilog is Implemented 
    SeriLog implemented as logging provider that replaces the default ILogger 
    Obviously cannot reach the AWS Cloudwatch (Env Var was created to AWS Loggroup)
    So will try to log to aws-logger-errors.txt

## Health Checks are Implemented
    /health/live
    /health/ready

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




