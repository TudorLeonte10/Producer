using System;

namespace Producer.src.Utils
{
    public class GenerateRandomVehicle
    {
        public readonly string[] vehicles = { "V-001", "V-002", "V-003", "V-004", "V-005", "V-006", "V-007", "V-008", "V-009", "V-0010" };

        private static readonly Random _random = new();

        public string GetRandomVehicle()
        {
            if (vehicles == null || vehicles.Length == 0)
                throw new InvalidOperationException("Vehicle list is empty.");

            int idx = _random.Next(vehicles.Length);
            return vehicles[idx];
        }
    }
}