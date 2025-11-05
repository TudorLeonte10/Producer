using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Application.Config
{
    public class ProducerSettings
    {
        public string OutputDir { get; set; } = @"C:\Producer\prod\inbox";
        public int MaxFileSizeMb { get; set; } = 5;
        public int RotationSeconds { get; set; } = 30;
        public int BackpressureThreshold { get; set; } = 100;
        public int IntervalMs { get; set; } = 0;
        public string[] VehicleIds { get; set; } = Array.Empty<string>();
    }
}
