# api-webinar-sample
Sample code used in the MEA webinar on the MCast™ API. This is mostly intended as a reference for any code supporting or written during the webinar, but it is also intended to be executable by itself. The webinar assumes that there exists a database with load observations in it and a schema for storing load forecasts, but no existing forecasts. In the webinar, we populated the load forecasts in the database using the MCast™ API. To execute this project directly you will need to:

1. Setup the YourGasUtility database on an existing SQL Server instance using the SQL scripts provided.
2. Update the values in Config.cs to match your environment.
