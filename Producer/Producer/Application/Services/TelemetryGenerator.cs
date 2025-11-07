using Microsoft.Extensions.Options;
using Producer.Application.Config;
using Producer.Domain.Entities;
using System;
using System.Collections.Generic;

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

        public TelemetryGenerator(IOptions<TelemetrySettings> options)
        {
            var cfg = options.Value;

            _baseSpeed = cfg.BaseSpeed;
            _speedVariation = cfg.SpeedVariation;
            _fuelConsumptionRate = cfg.FuelConsumptionRate;
            _tempBase = cfg.TempBase;
            _tempVariation = cfg.TempVariation;
            _recordInterval = TimeSpan.FromSeconds(cfg.RecordIntervalSeconds);
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
