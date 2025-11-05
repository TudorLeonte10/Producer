using Producer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Services
{
    public class TelemetryGenerator
    {
        private readonly Random _random = new();
        private readonly Dictionary<string, VehicleState> _state = new();

        private readonly double _baseSpeed;
        private readonly double _speedVariation;
        private readonly double _fuelConsumptionRate;
        private readonly double _tempBase;
        private readonly double _tempVariation;
        private readonly TimeSpan _recordInterval;

        public TelemetryGenerator(
            double baseSpeed = 70.0,
            double speedVariation = 15.0,
            double fuelConsumptionRate = 0.01,  
            double tempBase = 90.0,
            double tempVariation = 10.0,
            TimeSpan? recordInterval = null)
        {
            _baseSpeed = baseSpeed;
            _speedVariation = speedVariation;
            _fuelConsumptionRate = fuelConsumptionRate;
            _tempBase = tempBase;
            _tempVariation = tempVariation;
            _recordInterval = recordInterval ?? TimeSpan.FromSeconds(60);
        }

        public TelemetryRecord GenerateTelemetry(string vehicleId)
        {
            if (!_state.ContainsKey(vehicleId))
            {
                _state[vehicleId] = new VehicleState
                {
                    LastTimeStamp = DateTime.UtcNow,
                    FuelPct = 100.0
                };
            }

            var state = _state[vehicleId];

            state.LastTimeStamp = state.LastTimeStamp.Add(_recordInterval);

            double speed = _baseSpeed + (_random.NextDouble() * 2 - 1) * _speedVariation;
            double fuel = state.FuelPct - _fuelConsumptionRate * (speed / 100.0);
            double temp = _tempBase + (_random.NextDouble() * 2 - 1) * _tempVariation;

            if (fuel <= 5)
            {
                fuel = 100.0; 
            }
            else if (fuel < 20 && _random.NextDouble() < 0.01) 
            {
                fuel = 100.0;
            }

            state.FuelPct = fuel;

            return new TelemetryRecord
            {
                VehicleId = vehicleId,
                TsUtc = state.LastTimeStamp,
                SpeedKmh = Math.Round(speed, 2),
                FuelPct = Math.Round(fuel, 2),
                CoolantTempC = Math.Round(temp, 2)
            };
        }
    }
}
