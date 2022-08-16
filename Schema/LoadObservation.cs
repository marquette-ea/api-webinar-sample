using System;
using System.Collections.Generic;

namespace ApiSample.Schema
{
    public partial class LoadObservation
    {
        public DateTime Date { get; set; }
        public short OpArea { get; set; }
        public double Value { get; set; }

        public virtual OpArea OpAreaNavigation { get; set; } = null!;
    }
}
