using System;
using System.Collections.Generic;

namespace ApiSample.Schema
{
    public partial class OpArea
    {
        public OpArea()
        {
            LoadForecasts = new HashSet<LoadForecast>();
            LoadObservations = new HashSet<LoadObservation>();
        }

        public short Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<LoadForecast> LoadForecasts { get; set; }
        public virtual ICollection<LoadObservation> LoadObservations { get; set; }
    }
}
