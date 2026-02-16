# se3-customer-trx-aggregation-api
Api to aggregate customer financial transactions

This branch contains ongoing development for the Transaction Aggregation API

How to Build and Run 
===================== 
To Build the Docker Container : docker compose build --no-cache from the src directory
To Run ,from the src directory : run docker run --env-file .env -p 5000:8080 src-aggregation-api
Then in the Browser : http://localhost:5000/swagger/index.html


