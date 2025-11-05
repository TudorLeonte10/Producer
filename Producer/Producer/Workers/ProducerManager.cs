using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Producer.Application.Config;
using Producer.Application.Interfaces;
using Producer.Application.Services;
using Producer.Infrastructure.FileSystem;


namespace Producer.Workers
{
    public class ProducerManager : IHostedService
    {
        private readonly ProducerSettings _settings;
        private readonly IMetadataWriter _metadataWriter;
        private CancellationTokenSource? _cts;
        private Task? _runningTask;

        public ProducerManager(IOptions<ProducerSettings> options, IMetadataWriter metadataWriter)
        {
            _settings = options.Value;
            _metadataWriter = metadataWriter;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var vehicleIds = _settings.VehicleIds.Length > 0
                ? _settings.VehicleIds
                : new[] { "V001" };

            Console.WriteLine($"Starting {vehicleIds.Length} producers...");
            Console.WriteLine("Press Ctrl+C to stop.\n");

            _runningTask = Task.WhenAll(vehicleIds.Select(vehicleId => RunProducer(vehicleId, token)));
            return Task.CompletedTask;
        }

        private async Task RunProducer(string vehicleId, CancellationToken token)
        {
            var generator = new TelemetryGenerator();
            var writer = new FileWriterCoordinator(
                outputDirectory: _settings.OutputDir,
                metadataWriter: _metadataWriter,
                maxFileSizeMb: _settings.MaxFileSizeMb,
                rotationSeconds: _settings.RotationSeconds,
                backpressureThreshold: _settings.BackpressureThreshold
            );

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var record = generator.GenerateTelemetry(vehicleId);
                    await writer.WriteAsync(record, vehicleId, token);
                    //await Task.Delay(_settings.IntervalMs, token);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                await writer.CloseAsync(token);
                Console.WriteLine($"Producer for {vehicleId} stopped gracefully.");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Cancel();
            if (_runningTask != null)
                await _runningTask;
            Console.WriteLine("All producers stopped.");
        }
    }
}
