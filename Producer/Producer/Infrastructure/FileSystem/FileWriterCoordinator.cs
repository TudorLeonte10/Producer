using Producer.Application.Interfaces;
using Producer.Domain.Entities;
using Producer.src.Utils.Producer.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Producer.Infrastructure.FileSystem
{
    public class FileWriterCoordinator : IFileWriterService
    {
        private readonly FileRotator _rotator;
        private readonly IMetadataWriter _metadataWriter;
        private readonly int _backpressureThreshold;
        private ITelemetryStreamWriter? _streamWriter;
        private string _tmpPath = string.Empty;
        private string _finalPath = string.Empty;

        public FileWriterCoordinator(
            string outputDirectory,
            IMetadataWriter metadataWriter,
            int maxFileSizeMb = 10,
            int rotationSeconds = 30,
            int backpressureThreshold = 100)
        {
            Directory.CreateDirectory(outputDirectory);
            _metadataWriter = metadataWriter;
            _rotator = new FileRotator(outputDirectory, maxFileSizeMb, rotationSeconds);
            _backpressureThreshold = backpressureThreshold;
        }

        public async Task WriteAsync(TelemetryRecord record, string vehicleId, CancellationToken ct = default)
        {
            if (_streamWriter == null || _rotator.NeedsRotation())
            {
                await RotateAsync(vehicleId, ct);
            }

            int fileCount = Directory.GetFiles(_rotator.OutputDirectory, "*.jsonl*").Length;
            if (fileCount > _backpressureThreshold)
            {
                Console.WriteLine($"Backpressure detected ({fileCount} files). Throttling producer...");
                await Task.Delay(3000, ct);
            }

            await _streamWriter!.WriteRecordAsync(record);
        }

        private async Task RotateAsync(string vehicleId, CancellationToken ct)
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
                Console.WriteLine($"Closed file: {Path.GetFileName(_finalPath)}");
            }

            if (File.Exists(_finalPath))
            {
                FaultInjector.MaybeCorruptFile(_finalPath, 0.2);
            }

            (_streamWriter, _tmpPath, _finalPath) = await _rotator.CreateNewFileAsync(vehicleId, ct);

            Console.WriteLine($"Started new file: {Path.GetFileName(_finalPath)}");
        }

        public async Task CloseAsync(CancellationToken ct = default)
        {
            if (_streamWriter != null && !string.IsNullOrEmpty(_tmpPath))
            {
                await _streamWriter.CloseAsync();
                await SafeMoveAsync(_tmpPath, _finalPath);
                await _metadataWriter.WriteMetadataAsync(
                    _finalPath,
                    _streamWriter.RecordCount,
                    _streamWriter.UseCompression
                );
                Console.WriteLine($"Closed final file: {Path.GetFileName(_finalPath)}");
            }
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
    }
}
