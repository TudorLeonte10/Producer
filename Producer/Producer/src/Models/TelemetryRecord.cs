using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Producer.src.Models
{
    public class TelemetryRecord
    {
        [JsonPropertyName("vehicleId")]
        public string VehicleId { get; set; } = string.Empty;
        [JsonPropertyName("tsUtc")]
        public DateTime TsUtc { get; set; }
        [JsonPropertyName("speedKmh")]
        public double SpeedKmh { get; set; }
        [JsonPropertyName("fuelPct")]
        public double FuelPct { get; set; }
        [JsonPropertyName("coolantTempC")]
        public double CoolantTempC { get; set; }
    }
}
