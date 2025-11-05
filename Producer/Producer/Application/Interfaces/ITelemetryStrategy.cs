using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Interfaces
{
    public interface ITelemetryStrategy
    {
        TimeSpan GetNextInterval();
        string Name { get; }
    }
}
