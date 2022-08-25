
using System;
using System.Linq;
using System.Collections.Generic;
using dotenv.net;
using ApiSample.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiSample {
  record ApiForecastData ();
  class Program {
    static string ConnectionString => Environment.GetEnvironmentVariable("CONN_STRING")!;
    static DateOnly StartDate = new DateOnly(2022,01,01);

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
      throw new NotImplementedException();
    }

    static LoadForecast ApiRecordToDbRecords(ApiForecastData apiRecord, IDictionary<string, short> opAreaIDsByName) {
      throw new NotImplementedException();
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