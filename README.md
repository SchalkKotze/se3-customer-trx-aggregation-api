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

## Automapper Implemented
    To Map Requests to Commands
## fluentValidator Implemented
    To do validation of commands
## Logger 

## Health Checks Implemented
    /health/live
    /health/ready


