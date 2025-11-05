using Producer.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Strategies
{
    public class BurstStrategy : ITelemetryStrategy
    {
        private readonly Random _rnd = new();
        private int _counter = 0;

        public TimeSpan GetNextInterval()
        {
            if (_counter < _rnd.Next(5, 10))
            {
                _counter++;
                return TimeSpan.FromMilliseconds(_rnd.Next(1, 5)); 
            }
            else
            {
                _counter = 0;
                return TimeSpan.FromMilliseconds(_rnd.Next(300, 500)); 
            }
        }

        public string Name => "Burst";
    }
}
