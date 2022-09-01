
using System;
using System.Linq;
using System.Collections.Generic;
using dotenv.net;
using ApiSample.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace ApiSample {
  record ApiForecastData (
    string OperatingArea,
    DateOnly ForecastStartDate,
    string Idf,
    List<ApiForecastDataValue> LoadForecast
  ) {}

  record ApiForecastDataValue (
    DateOnly Date,
    int DaysOut,
    double Forecast
  ) {}

  class Program {
    static string ConnectionString => Environment.GetEnvironmentVariable("CONN_STRING")!;
    static string ApiKey => Environment.GetEnvironmentVariable("MCAST_API_KEY")!;
    static DateOnly StartDate = new DateOnly(2022,01,01);
    static readonly string MCastDomain = "demo-gas.mea-analytics.tools";
    static readonly HttpClient Client = new HttpClient();

    public static async Task Main(string[] args) {

      DotEnv.Load(new DotEnvOptions(probeForEnv:true, probeLevelsToSearch:5));

      var opAreas = await GetOpAreas();
      var latestDate = await LoadLatestDateFromDatabase();
      var minDate = latestDate?.AddDays(1) ?? StartDate;
      var recordsToStore = await GetNewForecastRecords(opAreas, minDate);

      var nRecordsStored = await StoreNewRecords(recordsToStore);
      Console.WriteLine($"Wrote {nRecordsStored} records to the database");
    }

    static async Task<IEnumerable<LoadForecast>> GetNewForecastRecords(IEnumerable<OpArea> opAreas, DateOnly minDate) {
      var newApiFcstRecords = await GetApiForecastData(opAreas, minDate);
      var opAreaIDsByName = opAreas.ToDictionary(keySelector: oa => oa.Name, elementSelector: oa => oa.Id);
      return from newRecord in newApiFcstRecords select ApiRecordToDbRecords(newRecord, opAreaIDsByName);
    }

    static async Task<IEnumerable<ApiForecastData>> GetApiForecastData(IEnumerable<OpArea> opAreas, DateOnly minDate) {
      Client.DefaultRequestHeaders.Accept.Clear();
      Client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

      var maxDate = DateTime.Today;

      var allForecasts = new List<ApiForecastData>();
      foreach (var opArea in opAreas) {
        Console.WriteLine($"Retrieving forecast data for {opArea.Name}...");

        var query = new Dictionary<string, string> {  
          ["operatingArea"] = opArea.Name,
          ["startDate"] = minDate.ToShortDateString(),
          ["endDate"] = maxDate.ToShortDateString(),
          ["idf"] = "Nom Plan",
        };
        var uri = QueryHelpers.AddQueryString($"https://{MCastDomain}/api/v1/daily/forecasted-load", query);
        var response = await Client.GetAsync(uri);
        Console.WriteLine(response.ToString());
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var fcsts = JsonConvert.DeserializeObject<List<ApiForecastData>>(json)!;
        allForecasts.AddRange(fcsts);
      }
      return allForecasts;
    }

    static LoadForecast ApiRecordToDbRecords(ApiForecastData apiRecord, IDictionary<string, short> opAreaIDsByName) {
      var fcstRecord = 
        new LoadForecast { 
          Date = apiRecord.ForecastStartDate.ToDateTime(TimeOnly.MinValue),
          OpArea = opAreaIDsByName[apiRecord.OperatingArea],
        };

      var valueRecords = 
        ( 
          from value in apiRecord.LoadForecast 
          select new LoadForecastValue {
            Horizon = (byte)value.DaysOut,
            Value = value.Forecast,
            ForecastNavigation = fcstRecord,
          }
        ).ToList();

      fcstRecord.LoadForecastValues = valueRecords;

      return fcstRecord;
    }

    // ---------------------------------------------
    // DB Operations:

    static YourGasUtilityContext GetCtx() {
      return new YourGasUtilityContext(
        new DbContextOptionsBuilder<YourGasUtilityContext>()
          .UseSqlServer(ConnectionString)
          .Options
      );
    } 

    static Task<List<OpArea>> GetOpAreas() { 
      var ctx = GetCtx();
      return ctx.OpAreas.ToListAsync();
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

    static Task<int> StoreNewRecords(IEnumerable<LoadForecast> fcsts) {
      var ctx = GetCtx();
      ctx.LoadForecasts.AddRange(fcsts);
      return ctx.SaveChangesAsync();
    }
  }
}