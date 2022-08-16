
using System;
using System.Linq;
using System.Collections.Generic;
using dotenv.net;
using ApiSample.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace ApiSample {
  record ForecastJson(
    string OperatingArea,
    DateOnly ForecastStartDate,
    DateTime UtcRetrievalTimestamp,
    DateTime UtcForecastTimestamp,
    bool IsPinned,
    string Idf,
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

    public static async Task Main(string[] args) {

      DotEnv.Load(new DotEnvOptions(probeForEnv:true, probeLevelsToSearch:5));

      Client.DefaultRequestHeaders.Accept.Clear();
      Client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

      var recordsToStore = await GetNewForecastRecords();

      var nRecordsStored = await StoreNewRecords(recordsToStore);

      Console.WriteLine($"Wrote {nRecordsStored} records to the database");
    }

    static async Task<IEnumerable<LoadForecast>> GetNewForecastRecords() {
      var opAreas = await GetOpAreas();
      var opAreaIDsByName = opAreas.ToDictionary(keySelector: oa => oa.Name, elementSelector: oa => oa.Id);
      var newApiFcstRecords = await GetApiForecasts(opAreas);
      return from newRecord in newApiFcstRecords select ApiRecordToDbRecords(newRecord, opAreaIDsByName);
    }

    static async Task<List<ForecastJson>> GetApiForecasts(IEnumerable<OpArea> opAreas) {
      var maxDate = DateOnly.FromDateTime(DateTime.Today);

      var allForecasts = new List<ForecastJson>();
      foreach (var opArea in opAreas) { 
        var latestDate = await LoadLatestDateFromDatabase(opArea.Id);
        var minDate = latestDate?.AddDays(1) ?? StartDate;

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
        var forecasts = JsonConvert.DeserializeObject<List<ForecastJson>>(json)!;
        allForecasts.AddRange(forecasts);
      }

      return allForecasts;
    }

    static LoadForecast ApiRecordToDbRecords(ForecastJson json, IDictionary<string, short> opAreaIDsByName) {
      var fcstRecord = 
        new LoadForecast { 
          Date = json.ForecastStartDate.ToDateTime(TimeOnly.MinValue),
          OpArea = opAreaIDsByName[json.OperatingArea],
        };

      var valueRecords = 
        ( 
          from value in json.LoadForecast 
          select new LoadForecastValue {
            Horizon = (byte)value.DaysOut,
            Value = value.Forecast,
            ForecastNavigation = fcstRecord,
          }
        ).ToList();

      fcstRecord.LoadForecastValues = valueRecords;

      return fcstRecord;
    }


    static Task<List<OpArea>> GetOpAreas() { 
      var ctx = GetCtx();
      return ctx.OpAreas.ToListAsync();
    }

    static YourGasUtilityContext GetCtx() {
      return new YourGasUtilityContext(
        new DbContextOptionsBuilder<YourGasUtilityContext>()
          .UseSqlServer(ConnectionString)
          .Options
      );
    }

    static async Task<DateOnly?> LoadLatestDateFromDatabase(short opArea) {
      var ctx = GetCtx();
      var lastDate = await
        ( 
          from row in ctx.LoadForecasts
          where row.OpArea == opArea
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