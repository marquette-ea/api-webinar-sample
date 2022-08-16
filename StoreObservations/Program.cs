
using System;
using System.Collections.Generic;
using dotenv.net;
using ApiSample.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace ApiSample {
  // The JSON type we receive from the MCast™ API
  record ObservedJson(
    string OperatingArea,
    DateOnly Date,
    double NetLoad,
    DateTime UtcRetrievalTimestamp
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

      var recordsToStore = await GetNewObservationRecords();

      var nRecordsStored = await StoreNewRecords(recordsToStore);
      Console.WriteLine($"Wrote {nRecordsStored} observation records to the database");
    }

    static YourGasUtilityContext GetCtx() {
      return new YourGasUtilityContext(
        new DbContextOptionsBuilder<YourGasUtilityContext>()
          .UseSqlServer(ConnectionString)
          .Options
      );
    }

    static Task<List<OpArea>> GetOpAreas() { 
      var ctx = GetCtx();
      return
        (
          from row in ctx.OpAreas
          select row
        ).ToListAsync();
    }

    static async Task<IEnumerable<LoadObservation>> GetNewObservationRecords() {
      var opAreas = await GetOpAreas();
      var opAreaIDsByName = opAreas.ToDictionary(keySelector: oa => oa.Name, elementSelector: oa => oa.Id);
      var newApiRecords = await GetApiObservations(opAreas);
      return from newRecord in newApiRecords select ApiRecordToDbRecord(newRecord, opAreaIDsByName);
    }

    static async Task<List<ObservedJson>> GetApiObservations(List<OpArea> opAreas) {
      var maxDate = DateOnly.FromDateTime(DateTime.Today);

      var allObservations = new List<ObservedJson>();
      foreach (var opArea in opAreas) { 
        var minDate = await LoadLatestDateFromDatabase(opArea.Id) ?? StartDate;

        var query = new Dictionary<string, string> {  
          ["operatingArea"] = opArea.Name,
          ["startDate"] = minDate.ToShortDateString(),
          ["endDate"] = maxDate.ToShortDateString(),
        };
        var uri = QueryHelpers.AddQueryString($"https://{MCastDomain}/api/v1/daily/observed-load", query);
        var response = await Client.GetAsync(uri);
        Console.WriteLine(response.ToString());
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var observations = JsonConvert.DeserializeObject<List<ObservedJson>>(json)!;
        allObservations.AddRange(observations);
      }

      return allObservations;
    }

    static async Task<DateOnly?> LoadLatestDateFromDatabase(short opArea) {
      var ctx = GetCtx();
      var lastDate = await
        ( 
          from row in ctx.LoadObservations
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

    static LoadObservation ApiRecordToDbRecord(ObservedJson newRecord, Dictionary<string, short> opAreas) {
      return new LoadObservation {
        Date = newRecord.Date.ToDateTime(TimeOnly.MinValue),
        OpArea = opAreas[newRecord.OperatingArea],
        Value = newRecord.NetLoad
      };
    }

    static Task<int> StoreNewRecords(IEnumerable<LoadObservation> records) {
      var ctx = GetCtx();
      ctx.LoadObservations.AddRange(records);
      return ctx.SaveChangesAsync();
    }
  }
}