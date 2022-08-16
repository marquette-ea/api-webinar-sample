
using System;
using System.Linq;
using System.Collections.Generic;
using dotenv.net;
using ApiSample.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiSample {
  class Program {
    static string ConnectionString => Environment.GetEnvironmentVariable("CONN_STRING")!;

    public static async Task Main(string[] args) {

      DotEnv.Load(new DotEnvOptions(probeForEnv:true, probeLevelsToSearch:5));

      // var recordsToStore = await GetNewForecastRecords();

      // var nRecordsStored = await StoreNewRecords(recordsToStore);
      // Console.WriteLine($"Wrote {nRecordsStored} records to the database");
    }

    static async Task<LoadForecast> GetNewForecastRecords() {
      throw new NotImplementedException();
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