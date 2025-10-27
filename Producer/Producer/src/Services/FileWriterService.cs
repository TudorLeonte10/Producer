using Producer.src.Models;
using Producer.src.Utils;
using Producer.src.Utils.Producer.src.Utils;
using System.Text;

namespace Producer.src.Services
{
    public class FileWriterService
    {
        private readonly string _outputDirectory;
        private readonly MetadataWriter _metadataWriter;
        private TelemetryStreamWriter? _streamWriter;  
        private readonly FileRotationManager _rotationManager;

        private string _tmpPath = string.Empty;
        private string _finalPath = string.Empty;
        private readonly int _backpressureThreshold;
        private readonly Random _random = new();

        public FileWriterService(
            string outputDirectory,
            bool useCompression = false, 
            int maxFileSizeMb = 5,
            int rotationSeconds = 30,
            int backpressureThreshold = 100)
        {
            _outputDirectory = outputDirectory;
            Directory.CreateDirectory(_outputDirectory);

            _metadataWriter = new MetadataWriter();
            _rotationManager = new FileRotationManager(maxFileSizeMb, TimeSpan.FromSeconds(rotationSeconds));
            _backpressureThreshold = backpressureThreshold;
        }

        public async Task WriteAsync(TelemetryRecord record, string vehicleId, CancellationToken ct = default)
        {
            if (_rotationManager.NeedsRotation())
                await RotateFileAsync(vehicleId, ct);

            if (_streamWriter != null)
                await _streamWriter.WriteRecordAsync(record);
        }

        private async Task RotateFileAsync(string vehicleId, CancellationToken ct)
        {
            var fileCount = Directory.GetFiles(_outputDirectory, "*.jsonl*").Length;
            if (fileCount > _backpressureThreshold)
            { 
                Console.WriteLine($"Backpressure: {fileCount} files pending. Slowing down producer...");
                await Task.Delay(3000, ct);
            }

            if (!string.IsNullOrEmpty(_tmpPath) && _streamWriter != null)
            {
                await _streamWriter.CloseAsync();
                await SafeMoveAsync(_tmpPath, _finalPath);
                await _metadataWriter.WriteMetadataAsync(
                    _finalPath,
                    _streamWriter.RecordCount,
                    _streamWriter.UseCompression
                );

                if (File.Exists(_finalPath))
                    FaultInjector.MaybeCorruptFile(_finalPath, 0.03); 
            }

            bool useCompression = _random.NextDouble() < 0.2;
            _streamWriter = new TelemetryStreamWriter(useCompression);

            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            string extension = useCompression ? ".jsonl.gz" : ".jsonl";
            string filename = $"telemetry_{timestamp}_{vehicleId}{extension}";
            _tmpPath = Path.Combine(_outputDirectory, filename + ".tmp");
            _finalPath = Path.Combine(_outputDirectory, filename);

            await _streamWriter.OpenAsync(_tmpPath);
            _rotationManager.SetCurrentFile(_streamWriter.BaseStream!);

            Console.WriteLine($"Started new file: {Path.GetFileName(_finalPath)}");
        }

        private static async Task SafeMoveAsync(string tmpPath, string finalPath)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    File.Move(tmpPath, finalPath);
                    return;
                }
                catch (IOException) when (i < 2)
                {
                    await Task.Delay(200);
                }
            }
        }

        public async Task CloseAsync(CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(_tmpPath) && _streamWriter != null)
            {
                await _streamWriter.CloseAsync();
                await SafeMoveAsync(_tmpPath, _finalPath);
                await _metadataWriter.WriteMetadataAsync(
                    _finalPath,
                    _streamWriter.RecordCount,
                    _streamWriter.UseCompression
                );
            }
        }
    }
}
