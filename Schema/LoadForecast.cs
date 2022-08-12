using System;
using System.Collections.Generic;

namespace ApiSample.Schema
{
    public partial class LoadForecast
    {
        public LoadForecast()
        {
            LoadForecastValues = new HashSet<LoadForecastValue>();
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public short OpArea { get; set; }

        public virtual OpArea OpAreaNavigation { get; set; } = null!;
        public virtual ICollection<LoadForecastValue> LoadForecastValues { get; set; }
    }
}
