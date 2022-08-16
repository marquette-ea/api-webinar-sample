using System;
using System.Collections.Generic;

namespace ApiSample.Schema
{
    public partial class LoadForecastValue
    {
        public int Forecast { get; set; }
        public byte Horizon { get; set; }
        public double Value { get; set; }

        public virtual LoadForecast ForecastNavigation { get; set; } = null!;
    }
}
