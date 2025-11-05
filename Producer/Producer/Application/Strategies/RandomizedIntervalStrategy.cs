using Producer.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Strategies
{
    public class RandomizedIntervalStrategy : ITelemetryStrategy
    {
        private readonly int _minMs;
        private readonly int _maxMs;
        private readonly Random _rnd = new();

        public RandomizedIntervalStrategy(int minMs, int maxMs)
        {
            _minMs = minMs;
            _maxMs = maxMs;
        }

        public TimeSpan GetNextInterval() => TimeSpan.FromMilliseconds(_rnd.Next(_minMs, _maxMs));
        public string Name => $"Random({_minMs}-{_maxMs}ms)";
    }
}
