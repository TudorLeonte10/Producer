using Producer.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Infrastructure.FileSystem
{
    public class FileRotator
    {
        private readonly FileRotationManager _rotationManager;
        private readonly Random _random = new();
        public string OutputDirectory { get; }

        public FileRotator(string outputDirectory, int maxFileSizeMb, int rotationSeconds)
        {
            OutputDirectory = outputDirectory;
            _rotationManager = new FileRotationManager(maxFileSizeMb, TimeSpan.FromSeconds(rotationSeconds));
        }

        public bool NeedsRotation()
        {
            return _rotationManager.NeedsRotation();
        }

        public async Task<(TelemetryStreamWriter writer, string tmpPath, string finalPath)> CreateNewFileAsync(
            string vehicleId,
            CancellationToken ct = default)
        {
            bool useCompression = _random.NextDouble() < 0.2;
            var writer = new TelemetryStreamWriter(useCompression);

            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            string extension = useCompression ? ".jsonl.gz" : ".jsonl";
            string filename = $"telemetry_{timestamp}_{vehicleId}{extension}";
            string tmpPath = Path.Combine(OutputDirectory, filename + ".tmp");
            string finalPath = Path.Combine(OutputDirectory, filename);

            await writer.OpenAsync(tmpPath);
            _rotationManager.SetCurrentFile(writer.BaseStream!);

            return (writer, tmpPath, finalPath);
        }
    }
}
