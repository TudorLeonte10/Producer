using Producer.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Strategies
{
    public class ConstantIntervalStrategy : ITelemetryStrategy
    {
        private readonly int _intervalMs;
        public ConstantIntervalStrategy(int intervalMs) => _intervalMs = intervalMs;
        public TimeSpan GetNextInterval() => TimeSpan.FromMilliseconds(_intervalMs);
        public string Name => $"Constant({_intervalMs}ms)";
    }
}
