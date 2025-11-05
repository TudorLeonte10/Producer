using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Producer.Domain.Models
{
    public class TelemetryMetadata
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";
        [JsonPropertyName("createdUtc")]
        public DateTime CreatedUTC { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("recordCount")]
        public int RecordCount { get; set; }
        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; } = string.Empty;
        [JsonPropertyName("encoding")]
        public string Encoding { get; set; } = "utf-8";
        [JsonPropertyName("compression")]
        public string Compression { get; set; } = "none";
    }
}
