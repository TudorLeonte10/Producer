using Producer.Application.Enums;
using Producer.Application.Interfaces;
using Producer.Application.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Factories
{
    public static class TelemetryStrategyFactory
    {
        public static ITelemetryStrategy Create(TelemetryStrategyType type)
        {
            return type switch
            {
                TelemetryStrategyType.Constant => new ConstantIntervalStrategy(5),
                TelemetryStrategyType.Random => new RandomizedIntervalStrategy(1, 50),
                TelemetryStrategyType.Burst => new BurstStrategy(),
                _ => new ConstantIntervalStrategy(5)
            };
        }
    }
}
