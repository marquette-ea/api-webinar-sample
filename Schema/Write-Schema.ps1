Remove-Item *.cs

dotnet ef dbcontext scaffold $env:CONN_STRING Microsoft.EntityFrameworkCore.SqlServer -n ApiSample.Schema --no-onconfiguring
