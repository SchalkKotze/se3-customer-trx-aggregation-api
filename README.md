# se3-customer-trx-aggregation-api
Api to aggregate customer financial transactions

This branch contains ongoing development for the Transaction Aggregation API

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

## How to Build and Run 
 
To Build the Docker Container : docker compose build --no-cache from the src directory
To Run ,from the src directory : run docker run --env-file .env -p 5000:8080 src-aggregation-api
Then in the Browser : http://localhost:5000/swagger/index.html

## How to Test

cd trx-aggreagtion-api.Test
dotnet test

3 Positive Tests
1 Negative Test

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

