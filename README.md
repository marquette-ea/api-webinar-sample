
# api-webinar-sample

Sample code used in the MEA webinar on the MCast™ API. This is mostly intended as a reference for any code supporting or written during the webinar, but it is also intended to be executable by itself. The webinar assumes that there exists a database with load observations in it and a schema for storing load forecasts, but no existing forecasts. In the webinar, we populated the load forecasts in the database using the MCast™ API. 

## Project Structure

There are 4 folders in this repository

### StoreForecasts

This folder contains the code that was developed during the webinar in Program.cs. It reads from the database the latest forecast data available (if any), then retrieves any newer data from the API and stores it to the database.

### Schema 

This folder contains autogenerated code for interacting with the database using Entity Framework (Microsoft.EntityFrameworkCore.SqlServer). If you change the database schema, you will need to  update the autogenerated code. To do that you have to install the dotnet ef tool (run `dotnet tool restore` in the root folder), and then run the Write-Schema.ps1 PowerShell script. (Note that script needs a `CONN_STRING` environment variable set to provide the connection string to the database you're connecting to).

### SQL

This folder contains the CreateDatabase.sql script to create the YourGasUtility database that this project populates data for.  In the webinar we assume that this database already exists, but this script is needed if you're executing this repository from scratch (see "Getting Started" below).

### StoreObservations 

This folder contains code similar to what was developed during the webinar. It is used to populate the Observations data in the database. In the webinar we assume that the database already exists and that it contains observations, but this project is needed if you're executing this repository from scratch (see "Getting Started" below).

## Getting Started

To run the StoreForecasts project that was created in the webinar, you will need a SQL Server instance and then will need to create and populate the database used by the StoreForecasts project.  To do that, you will need to:

1. Run the SQL/CreateDatabase.sql script on your SQL Server instance. You'll also have to edit the list of OpAreas defined in that script to match your own system. You can use the operating-areas API route and run it via the API Reference page if you want to see the names for each op area that the MCast™ API will recognize. This script will create a new database named YourGasUtility.
2. Setup environment variables for `CONN_STRING` (the connection string to the new YourGasUtility database on your SQL Server instance) and `MCAST_API_KEY` (the API key you can get from the MCast™ web interface). You can optionally use a ".env" file to achieve this. To do that, rename `sample.env` to just `.env`, and then fill in those values in the file. You may also choose to modify the file permissions so that only you have permissions to that file. All `.env` files besides `sample.env` are in the .gitignore file, so it will not be committed to version control.
3. Run the StoreObservations project to populate observed values in the YourGasUtility database using the MCast™ API.  To do this, start in the StoreObservations folder, and run `dotnet run`.
