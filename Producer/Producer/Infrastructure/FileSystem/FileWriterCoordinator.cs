using Producer.Application.Config;
using Producer.Application.Interfaces;
using Producer.Domain.Entities;
using Producer.src.Utils.Producer.src.Utils;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Producer.Infrastructure.FileSystem
{
    public class FileWriterCoordinator : IFileWriterService
    {
        private readonly FileRotator _rotator;
        private readonly IMetadataWriter _metadataWriter;
        private readonly int _backpressureThreshold;
        private readonly double _faultInjectionProbability = 0.03;
        private ITelemetryStreamWriter? _streamWriter;
        private string _tmpPath = string.Empty;
        private string _finalPath = string.Empty;

        public FileWriterCoordinator(IOptions<ProducerSettings> options, IMetadataWriter metadataWriter)
        {
            var settings = options.Value;
            Directory.CreateDirectory(settings.OutputDir);

            _metadataWriter = metadataWriter;
            _rotator = new FileRotator(settings.OutputDir, settings.MaxFileSizeMb, settings.RotationSeconds);
            _backpressureThreshold = settings.BackpressureThreshold;
        }

        public async Task WriteAsync(TelemetryRecord record, string vehicleId, CancellationToken ct)
        {
            try
            {
                if (_streamWriter == null || _rotator.NeedsRotation())
                {
                    await RotateAsync(vehicleId, ct);
                }

                int fileCount = Directory.GetFiles(_rotator.OutputDirectory, "*.jsonl*").Length;
                if (fileCount > _backpressureThreshold)
                {
                    Console.WriteLine($"Backpressure detected ({fileCount} files). Slowing down producer...");
                    await Task.Delay(2000, ct);
                }

                await _streamWriter!.WriteRecordAsync(record);
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"Vehicle {record.VehicleId}: {ioEx.Message}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.WriteLine($"Unauthorized Acces {uaEx.Message}");
            }
        }

        private async Task RotateAsync(string vehicleId, CancellationToken ct)
        {
            try
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

                    if (File.Exists(_finalPath))
                        FaultInjector.MaybeCorruptFile(_finalPath, _faultInjectionProbability);
                }

                (_streamWriter, _tmpPath, _finalPath) = await _rotator.CreateNewFileAsync(vehicleId, ct);
                Console.WriteLine($" Started new file: {Path.GetFileName(_finalPath)}");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"{ioEx.Message}");
            }
        }

        public async Task CloseAsync(CancellationToken ct)
        {
            if (_streamWriter == null || string.IsNullOrEmpty(_tmpPath))
                return;

            try
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
            catch (IOException ioEx)
            {
                Console.WriteLine($"{ioEx.Message}");
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

            Console.WriteLine($"[ERROR] Failed to move {tmpPath} → {finalPath}");
        }
    }
}
