Remove-Item *.cs

dotnet ef dbcontext scaffold "Server=MSSQL03;Database=YourGasUtility;Trusted_Connection=true;" Microsoft.EntityFrameworkCore.SqlServer -n ApiSample.Schema --no-onconfiguring
