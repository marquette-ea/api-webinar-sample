
using System;
using System.Collections.Generic;
using dotenv.net;
using ApiSample.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ApiSample {
  record ForecastJson(
    string OperatingArea,
    DateOnly ForecastStartDate,
    DateTime UtcRetrievalTimestamp,
    DateTime UtcForecastTimestamp,
    bool IsPinned,
    int Idf,
    List<ForecastValueJson> LoadForecast
  ) {}

  record ForecastValueJson(
    DateOnly Date,
    int DaysOut,
    double Forecast
  ) {} 

  class Program {
    static string ConnectionString => Environment.GetEnvironmentVariable("CONN_STRING")!;
    static string ApiKey => System.Environment.GetEnvironmentVariable("MCAST_API_KEY")!;
    static readonly string MCastDomain = "demo-gas.mea-analytics.tools";
    static DateOnly StartDate = new DateOnly(2022,01,01);
    static readonly HttpClient Client = new HttpClient();

    // This lets us use PascalCase for our field names in the records defined above, which is standard for C#
    // even though the JSON we receive from the API uses camelCase.
    static readonly JsonSerializerSettings SerializerSettings = 
      new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    public static async Task Main(string[] args) {

      DotEnv.Load(new DotEnvOptions(probeForEnv:true, probeLevelsToSearch:5));

      Client.DefaultRequestHeaders.Accept.Clear();
      Client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

      // var recordsToStore = await GetNewForecastRecords();

      // var (nFcstRecordsStored, nFcstValueRecordsStored) = await StoreNewRecords(fcstRecords, fcstValueRecords);
      // Console.WriteLine($"Wrote {nFcstRecordsStored} forecast records to the database");
      // Console.WriteLine($"Wrote {nFcstValueRecordsStored} forecast value records to the database");
    }

    record ForecastData(IEnumerable<LoadForecast> FcstRecords, IEnumerable<LoadForecastValue> FcstValueRecords) { }

    static async Task<ForecastData> GetNewForecastRecords() {
      throw new NotImplementedException();
    }

    static YourGasUtilityContext GetCtx() {
      return new YourGasUtilityContext(
        new DbContextOptionsBuilder<YourGasUtilityContext>()
          .UseSqlServer(ConnectionString)
          .Options
      );
    }

    static async Task<DateOnly?> LoadLatestDateFromDatabase() {
      var ctx = GetCtx();
      var lastDate = await
        ( 
          from row in ctx.LoadForecasts
          orderby row.Date descending
          select (DateTime?)row.Date
        ).FirstOrDefaultAsync();

      if (lastDate.HasValue) {
        return DateOnly.FromDateTime(lastDate.Value);
      }
      else {
        return null;
      }
    }

    static async Task<(int, int)> StoreNewRecords(ForecastData fcsts) {
      var ctx = GetCtx();
      ctx.LoadForecasts.AddRange(fcsts.FcstRecords);
      var nFcstRecordsStored = await ctx.SaveChangesAsync();
      ctx.LoadForecastValues.AddRange(fcsts.FcstValueRecords);
      var nFcstValueRecordsStored = await ctx.SaveChangesAsync();
      return (nFcstRecordsStored, nFcstValueRecordsStored);
    }
  }
}